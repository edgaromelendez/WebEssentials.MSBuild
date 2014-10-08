using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yahoo.Yui.Compressor.Build.MsBuild;
using Yahoo.Yui.Compressor;

namespace WebEssentials.MSBuild
{
    public class CssBundleAdapterCompressorTask:CompressorTask
    {
        private readonly ICssCompressor _compressor;
        
        public CssBundleAdapterCompressorTask() : this(new CssCompressor())
        {
        }

        public CssBundleAdapterCompressorTask(ICssCompressor compressor)
            : base(compressor)
        {
            this._compressor = compressor;
        }
        
        public override bool Execute()
        {
            this._compressor.RemoveComments = !this.PreserveComments;
            return base.Execute();
        }
        
        public bool PreserveComments { get; set; }
    }
}
