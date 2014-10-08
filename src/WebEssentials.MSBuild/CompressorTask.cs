using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
﻿using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Yahoo.Yui.Compressor;
using Yahoo.Yui.Compressor.Build;
using Yahoo.Yui.Compressor.Build.MsBuild;
using System.Xml.Linq;
using System.IO;

namespace WebEssentials.MSBuild
{
    public abstract class CompressorTask : Task
    {
        protected readonly CompressorTaskEngine TaskEngine;

        public string LoggingType { get; set; }

        [Required]
        public ITaskItem SourceFile { get; set; }

        [Required]
        public string OutputFile { get; set; }

        public string CompressionType { get; set; }

        public string EncodingType { get; set; }

        public bool DeleteSourceFiles { get; set; }

        public int LineBreakPosition { get; set; }

        protected CompressorTask(ICompressor compressor)
        {
            TaskEngine = new CompressorTaskEngine(new MsBuildLogAdapter(Log), compressor) { SetTaskEngineParameters = this.SetTaskEngineParameters };
            DeleteSourceFiles = false;
            LineBreakPosition = -1;
        }

        public override bool Execute()
        {
            return this.TaskEngine.Execute();
        }

        protected virtual void SetTaskEngineParameters()
        {
            this.TaskEngine.CompressionType = this.CompressionType;
            this.TaskEngine.DeleteSourceFiles = this.DeleteSourceFiles;
            this.TaskEngine.EncodingType = this.EncodingType;
            this.TaskEngine.LineBreakPosition = this.LineBreakPosition;
            this.TaskEngine.LoggingType = this.LoggingType;
            
            var fileSpecs = new List<FileSpec>();

            string bundleFile = SourceFile.ItemSpec;
            XDocument doc = null;
            string contents = File.ReadAllText(bundleFile);
            doc = XDocument.Parse(contents);

            SourceFile.GetMetadata("CompressionType");
            XElement element = null;

            element = doc.Descendants("outputDirectory").FirstOrDefault();
            if (element != null)
                this.TaskEngine.OutputFile = element.Value;

            string compressionType = "standard";
            element = doc.Descendants("minify").FirstOrDefault();
            if (element != null)
                compressionType =  element.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ? "standard" : "none";

            this.TaskEngine.CompressionType = compressionType;

            string resourceName = Path.GetFileNameWithoutExtension(bundleFile);
            string outputFileName = String.Concat(Path.GetFileNameWithoutExtension(resourceName),".min", Path.GetExtension(resourceName));
            this.TaskEngine.OutputFile = outputFileName;

            IEnumerable<string> files = doc.Descendants("file").Select(s => s.Value);

            foreach (var sourceFile in files)
            {
                fileSpecs.Add(new FileSpec(sourceFile, String.Empty));
            }

            this.TaskEngine.SourceFiles = fileSpecs.ToArray();
        }
    }
}
