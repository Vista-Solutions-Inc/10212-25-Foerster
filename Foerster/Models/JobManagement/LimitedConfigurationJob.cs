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
    [Description("Limited Configuration Job")]
    public class LimitedConfigurationJob : BaseJMJob
    {

        public override bool HasStepStructure => false;
        public override bool HasStreamStructure => true;
        public override bool PredefinedTaskStructure => true;

        public override bool HasInitializerStructure => false;
        public override bool HasFinalizerStructure => true;

        public override Type JobType { get { return typeof(LimitedConfigurationJob); } }
        public override List<Type> SupportedTaskTypes { get { return [typeof(ImageCaptureTask)]; } }

        public LimitedConfigurationJob(string saveJobFolderPath, string name, string id, ImageSource newJobImage) : base(saveJobFolderPath, name, id, newJobImage) 
        { 
            this.ModifyStep(this, new StepManagementEventArgs("Main_Step", "Insert at the end of the step list."));

        }

        public LimitedConfigurationJob(string saveJobFolderPath) : base(saveJobFolderPath) 
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
