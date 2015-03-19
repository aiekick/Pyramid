﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pyramid
{
    class AMDDriverResultSet : IResultSet
    {
        private AMDDriverResultsPanel m_Results = new AMDDriverResultsPanel();

        public string Name { get { return "AMDDXX"; } }
        public Control AnalysisPanel { get { return null; } }
        public Control ResultsPanel { get { return m_Results; } }
  
        public void Add( IAMDShader sh )
        {
            m_Results.AddResult(sh);
        }
    };

    class AMDDriverBackend : IBackend
    {
        private ID3DCompiler m_FXC  = null;
        private IAMDDriver m_Driver = null;

        public string Name { get { return "AMDDXX"; } }

        public AMDDriverBackend( IAMDDriver driver, ID3DCompiler fxc  )
        {
            m_FXC = fxc;
            m_Driver = driver;
        }

        public IResultSet Compile(string shader, ICompileOptions opts)
        {
            if (opts.Language != Languages.HLSL)
                return null;

            try
            {
                IDXShaderBlob blob;
                string messages;
                if (!m_FXC.Compile(shader, opts as IHLSLOptions, out blob, out messages))
                    return null;

                IDXShaderBlob exe = blob.GetExecutableBlob();
                if (exe == null)
                    return null;

                byte[] bytes = exe.ReadBytes();

                AMDDriverResultSet rs = new AMDDriverResultSet();

                foreach (IAMDAsic a in m_Driver.Asics)
                {
                    IAMDShader sh = m_Driver.CompileDXBlob(a, bytes);
                    rs.Add(sh);
                }

                return rs;
            }
            catch( System.Exception ex )
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}