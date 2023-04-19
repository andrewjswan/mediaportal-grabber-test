﻿using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net;
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

    private readonly bool autoStart;

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
      // frmTestGrabberMP1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1033, 315);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.button1);
      this.Name = "frmTestGrabberMP1";
      this.Text = "TestGrabberMP1";
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
        _asmHelper = new AsmHelper(CSScript.Load(scriptFileName, null, false));
        _grabber = (IIMDBScriptGrabber)_asmHelper.CreateObject("Grabber");
      }
      catch (Exception ex)
      {
        Log.Error("InfoGrabber LoadScript() - file: {0}, message : {1}", scriptFileName, ex.Message);
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
          return;
        }

        try
        {
          //add your test titles to this ArrayList 
          ArrayList elements = new ArrayList();
          foreach (string movieTitle in movieTitles)
          {
            _grabber.FindFilm(movieTitle, 5, elements);
            IMDBMovie movieDetails = new IMDBMovie();
            if (elements.Count > 0)
            {
              if (_grabber.GetDetails((IMDB.IMDBUrl)elements[0], ref movieDetails))
              {
                //only first element
                textBox1.Text = textBox1.Text + Environment.NewLine + _grabber.GetName() + Environment.NewLine;
                textBox1.Text = textBox1.Text + movieDetails.Title + Environment.NewLine + movieDetails.Year + Environment.NewLine + movieDetails.Genre;
                break;
              }
            }
          }
        }
        catch
        {
        }
      }
    }

    void Button1_Click(object sender, EventArgs e)
    {
      RunGrabber();
      if (autoStart)
      {
        System.Environment.Exit(0);
      }
    }
  }
}
