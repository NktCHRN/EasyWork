using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Other
{
    public class FileModelExtended
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int TaskId { get; set; }

        public long Size { get; set; }
    }
}
