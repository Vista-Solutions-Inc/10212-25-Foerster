using Jobs.BaseClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TaskToolBox.Tasks.Acquisition;

namespace Foerster.Models.JobManagement
{
    [Description("Full Configuration Job")]
    public class FullConfigurationJob : BaseJMJob
    {

        public override bool HasStepStructure => true;
        public override bool HasStreamStructure => true;
        public override bool PredefinedTaskStructure => true;

        public override bool HasInitializerStructure => true;
        public override bool HasFinalizerStructure => true;

        public override Type JobType { get { return typeof(FullConfigurationJob); } }
        public override List<Type> SupportedTaskTypes { get { return [typeof(ImageCaptureTask)]; } }

        public FullConfigurationJob(string saveJobFolderPath, string name, string id, ImageSource newJobImage) : base(saveJobFolderPath, name, id, newJobImage) 
        { }

        public FullConfigurationJob(string saveJobFolderPath) : base(saveJobFolderPath) 
        { }

        public override void CompleteJobCycle()
        {
            throw new NotImplementedException();
        }
        public override string? LoadForOperation(string? jobFolderPath = null)
        {
            return base.LoadForOperation(jobFolderPath);
        }
        public override string? LoadPartForOperation(string? jobFolderPath = null)
        {
            _part = new JobPart();
            return null;
        }
    }
}
