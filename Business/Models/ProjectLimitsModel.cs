using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class ProjectLimitsModel
    {
        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxToDo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxInProgress { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxValidate { get; set; }
    }
}
