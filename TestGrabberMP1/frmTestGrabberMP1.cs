﻿using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

using CSScriptLibrary;

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Video.Database;

namespace TestGrabberMP1
{
  public partial class FrmTestGrabberMP1 : Form
  {
    private Button button1;
    private TextBox textBox1;

    private readonly IContainer components = null;

    private static IIMDBScriptGrabber _grabber;
    private static AsmHelper _asmHelper;
    private static IIMDBInternalActorsScriptGrabber InternalActorsGrabber;
    private static InternalCSScriptGrabbersLoader.Movies.IInternalMovieImagesGrabber InternalMovieImagesGrabber;

    private readonly bool autoStart;
    private readonly bool errorReporting;
    private bool haveError;

    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Código generado por el Diseñador de Windows Forms

    /// <summary>
    /// Método necesario para admitir el Diseñador. No se puede modificar
    /// el contenido del método con el editor de código.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmTestGrabberMP1));
      this.button1 = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(12, 283);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(60, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "Start";
      this.button1.UseVisualStyleBackColor = true;
      // 
      // textBox1
      // 
      this.textBox1.Location = new System.Drawing.Point(12, 12);
      this.textBox1.Multiline = true;
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(1009, 265);
      this.textBox1.TabIndex = 1;
      // 
      // FrmTestGrabberMP1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1033, 315);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.button1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "FrmTestGrabberMP1";
      this.Text = "Test | Grabbers MP1";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    public FrmTestGrabberMP1()
    {
      // .NET 4.0: Use TLS v1.2. Many download sources no longer support the older and now insecure TLS v1.0/1.1 and SSL v3.
      ServicePointManager.SecurityProtocol = (SecurityProtocolType)0xc00;

      InitializeComponent();
      this.Load += FrmTestGrabberMP1_Load;
      Log.SetConfigurationMode();

      string[] args = Environment.GetCommandLineArgs();
      autoStart = args.Length > 1;
      errorReporting = args.Length > 2;
      haveError = false;

      if (autoStart)
      {
        Button1_Click(this, new EventArgs());
      }
    }

    void FrmTestGrabberMP1_Load(object sender, EventArgs e)
    {
      this.button1.Click += Button1_Click;
    }

    bool LoadScript(string grabber)
    {
      string scriptFileName = grabber;

      // Script support script.csscript
      if (!File.Exists(scriptFileName))
      {
        Log.Error("InfoGrabber LoadScript() - grabber script not found: {0}", scriptFileName);
        return false;
      }

      try
      {
        _asmHelper = new AsmHelper(CSScript.LoadFile(scriptFileName, null, false));
        _grabber = (IIMDBScriptGrabber)_asmHelper.CreateObject("Grabber");
      }
      catch (Exception ex)
      {
        haveError = true;
        Log.Error("InfoGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
        return false;
      }

      return true;
    }

    private static bool LoadScript()
    {
      string scriptFileName = AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\scripts\InternalActorMoviesGrabber.csscript";

      // Script support script.csscript
      if (!File.Exists(scriptFileName))
      {
        Log.Error("InternalActorMoviesGrabber LoadScript() - grabber script not found: {0}", scriptFileName);
        return false;
      }

      try
      {
        _asmHelper = new AsmHelper(CSScript.LoadFile(scriptFileName, null, false));
        InternalActorsGrabber = (IIMDBInternalActorsScriptGrabber)_asmHelper.CreateObject("InternalActorsGrabber");
      }
      catch (Exception ex)
      {
        Log.Error("InternalActorMoviesGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
        return false;
      }

      return true;
    }

    private static bool LoadThumbScript()
    {
      string scriptFileName = AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\scripts\InternalMovieImagesGrabber.csscript";

      // Script support script.csscript
      if (!File.Exists(scriptFileName))
      {
        Log.Error("InternalMovieImagesGrabber LoadScript() - grabber script not found: {0}", scriptFileName);
        return false;
      }

      try
      {
        _asmHelper = new AsmHelper(CSScript.LoadFile(scriptFileName, null, false));
        InternalMovieImagesGrabber = (InternalCSScriptGrabbersLoader.Movies.IInternalMovieImagesGrabber)_asmHelper.CreateObject("MovieImagesGrabber");
      }
      catch (Exception ex)
      {
        Log.Error("InternalMovieImagesGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
        return false;
      }

      return true;
    }

    void RunGrabber()
    {
      string[] grabbers = Utils.GetFiles(AppDomain.CurrentDomain.BaseDirectory + @"..\..\..\scripts\MovieInfo\");
      if (grabbers.Length == 0)
      {
        textBox1.Text = "Grabbers not found...";
        return;
      }

      ArrayList movieTitles = new ArrayList() { "Avatar" };
      textBox1.Text = "";

      foreach (string grabber in grabbers)
      {
        if (!LoadScript(grabber))
        {
          haveError = true;
          return;
        }

        try
        {
          //add your test titles to this ArrayList 
          foreach (string movieTitle in movieTitles)
          {
            Log.Info("----------------------------------------------------------------------");
            Log.Info("--- {0} - {1}", _grabber.GetName() , movieTitle);
            Log.Info("--- Grabber: {0}", Path.GetFileName(grabber));
            Log.Info("----------------------------------------------------------------------");

            ArrayList elements = new ArrayList();
            _grabber.FindFilm(movieTitle, 5, elements);
            Log.Info(string.Empty);

            IMDBMovie movieDetails = new IMDBMovie();
            if (elements.Count > 0)
            {
              if (_grabber.GetDetails((IMDB.IMDBUrl)elements[0], ref movieDetails))
              {
                //only first element
                textBox1.Text = textBox1.Text + Environment.NewLine + _grabber.GetName() + Environment.NewLine;
                textBox1.Text = textBox1.Text + movieDetails.Title + Environment.NewLine + movieDetails.Year + Environment.NewLine + movieDetails.Genre;
              }
            }
            Log.Info("--- END --------------------------------------------------------------");
            Log.Info(string.Empty);

            Thread.Sleep(1000);
          }
        }
        catch
        {
          haveError = true;
        }
      }

      IMDBMovie movie = new IMDBMovie
      {
        IMDBNumber = "tt0499549"
      };

      if (LoadScript())
      {
        ArrayList actorList;
        Log.Info("----------------------------------------------------------------------");
        Log.Info("--- GetIMDBMovieActorsList: {0}", movie.IMDBNumber);
        Log.Info("----------------------------------------------------------------------");
        try
        {
          actorList = InternalActorsGrabber.GetIMDBMovieActorsList(movie.IMDBNumber, true);
          foreach (string actor in actorList)
          {
            Log.Info("--- {0} - {1}", "Found", actor);
          }
        }
        catch (Exception ex)
        {
          haveError = true;
          Log.Error("--- {0}", ex.Message);
          Log.Info("--- ROLLBACK TO DEFAULT ----------------------------------------------");
          actorList = new ArrayList { "Bruce Willis", "nm0000246" };
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);

        int i = 0;
        foreach (string actor in actorList)
        {
          Log.Info("----------------------------------------------------------------------");
          Log.Info("--- GetActorDetails: {0} - {1}", movie.IMDBNumber, actor);
          Log.Info("----------------------------------------------------------------------");

          if (InternalActorsGrabber.GetActorDetails(new IMDB.IMDBUrl(actor, actor, "TEST"), out IMDBActor actorData))
          {
              textBox1.Text = textBox1.Text + Environment.NewLine + movie.IMDBNumber + Environment.NewLine;
              textBox1.Text = textBox1.Text + actorData.IMDBActorID + Environment.NewLine + actorData.Name + Environment.NewLine + actorData.MiniBiography;
          }
          Log.Info("--- END --------------------------------------------------------------");
          Log.Info(string.Empty);

          Thread.Sleep(1000);

          i++;
          if (i > 2)
            break;
        }

        if (actorList.Count > 0)
        {
          Log.Info("----------------------------------------------------------------------");
          Log.Info("--- FindIMDBActor: {0} - {1}", "Bruce Willis");
          Log.Info("----------------------------------------------------------------------");
          actorList = InternalActorsGrabber.FindIMDBActor("https://www.imdb.com/find/?s=nm&q=Bruce%20Willis");
          foreach (IMDB.IMDBUrl actor in actorList)
          {
            Log.Info("--- {0} - {1} - {2}", "Found", actor.Title, actor.URL);
          }
          Log.Info("--- END --------------------------------------------------------------");
          Log.Info(string.Empty);
        }

        try
        {
          Log.Info("----------------------------------------------------------------------");
          Log.Info("--- GetPlotImdb: {0}", movie.IMDBNumber);
          Log.Info("----------------------------------------------------------------------");
          InternalActorsGrabber.GetPlotImdb(ref movie);
          Log.Info("--- {0} - {1}", "Plot", movie.PlotOutline.Replace("\n", " "));
          Log.Info("--- {0} - {1}", "Plot", movie.Plot.Replace("\n", " "));
        }
        catch (Exception ex)
        { 
          haveError = true;
          Log.Error("--- {0}", ex.Message);
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);

        try
        {
          Log.Info("----------------------------------------------------------------------");
          Log.Info("--- GetThumbImdb: {0}", movie.IMDBNumber);
          Log.Info("----------------------------------------------------------------------");
          Log.Info("--- {0} - {1}", "Thumb", InternalActorsGrabber.GetThumbImdb(movie.IMDBNumber));
        }
        catch
        {
          haveError = true;
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);
      }

      if (LoadThumbScript())
      {
        Log.Info("----------------------------------------------------------------------");
        Log.Info("--- GetIMDBImages: {0}", movie.IMDBNumber);
        Log.Info("----------------------------------------------------------------------");
        ArrayList imageList = InternalMovieImagesGrabber.GetIMDBImages(movie.IMDBNumber, false);
        foreach (string image in imageList)
        {
          Log.Info("--- {0} - {1}", "Found", image);
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);

        Log.Info("----------------------------------------------------------------------");
        Log.Info("--- GetIMPAwardsImages: {0}", movie.IMDBNumber);
        Log.Info("----------------------------------------------------------------------");
        imageList = InternalMovieImagesGrabber.GetIMPAwardsImages("Avatar", movie.IMDBNumber);
        foreach (string image in imageList)
        {
          Log.Info("--- {0} - {1}", "Found", image);
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);
        Log.Info("----------------------------------------------------------------------");
        Log.Info("--- GetTmdbCoverImages: {0}", movie.IMDBNumber);
        Log.Info("----------------------------------------------------------------------");
        imageList = InternalMovieImagesGrabber.GetTmdbCoverImages("Avatar", movie.IMDBNumber);
        foreach (string image in imageList)
        {
          Log.Info("--- {0} - {1}", "Found", image);
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);
        Log.Info("----------------------------------------------------------------------");
        Log.Info("--- GetTmdbFanartByApi: {0}", movie.IMDBNumber);
        Log.Info("----------------------------------------------------------------------");
        Log.Info("--- ID ---------------------------------------------------------------");
        imageList = InternalMovieImagesGrabber.GetTmdbFanartByApi(-1, movie.IMDBNumber, "Avatar", true, 5, string.Empty, 
                                                                  out string _fileFanArtDefault, out string _fanartUrl);
        Log.Info("--- {0} = {1}", _fileFanArtDefault, _fanartUrl);
        foreach (string image in imageList)
        {
          Log.Info("--- {0} - {1}", "Found", image);
        }
        Log.Info("--- TITLE ------------------------------------------------------------");
        imageList = InternalMovieImagesGrabber.GetTmdbFanartByApi(-1, string.Empty, "Avatar", true, 5, string.Empty, 
                                                                  out string _fileFanArtDefaultTitle, out string _fanartUrlTitle);
        Log.Info("--- {0} = {1}", _fileFanArtDefaultTitle, _fanartUrlTitle);
        foreach (string image in imageList)
        {
          Log.Info("--- {0} - {1}", "Found", image);
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);
        Log.Info("----------------------------------------------------------------------");
        Log.Info("--- GetTmdbActorImage: {0} {1}", "nm0000246", "Bruce Willis");
        Log.Info("----------------------------------------------------------------------");
        imageList = InternalMovieImagesGrabber.GetTmdbActorImage("Bruce Willis");
        foreach (string image in imageList)
        {
          Log.Info("--- {0} - {1}", "Found", image);
        }
        Log.Info("--- ID ---------------------------------------------------------------");
        imageList = InternalMovieImagesGrabber.GetTmdbActorImage("nm0000246");
        foreach (string image in imageList)
        {
          Log.Info("--- {0} - {1}", "Found", image);
        }
        Log.Info("--- END --------------------------------------------------------------");
        Log.Info(string.Empty);
      }
      else
      {
        haveError = true;
      }
    }

    void Button1_Click(object sender, EventArgs e)
    {
      button1.Enabled = false;
      RunGrabber();
      button1.Enabled = true;
      if (autoStart)
      {
        if (errorReporting && haveError)
        {
          System.Environment.Exit(1);
        }
        else
        {
          System.Environment.Exit(0);
        }
      }
    }
  }
}
