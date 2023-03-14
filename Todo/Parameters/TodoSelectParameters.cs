using System;
using System.Text.RegularExpressions;

namespace Todo.Parameters
{
    public class TodoSelectParameters
    {
        public string name {get; set;}
        public bool? enable { get; set; }
        public DateTime? InsertTime { get; set; }
        public int? minOrder { get; set; }
        public int? maxOrder { get; set; }

        private string _order;

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string Order
        {
            get { return _order; }
            set 
            {
                //2-3
                Regex regex = new Regex(@"^\d*-\d*$");
                if (regex.Match(value).Success )
                {
                    minOrder = Int32.Parse(value.Split('-')[0]);
                    maxOrder = Int32.Parse(value.Split('-')[1]);
                }
                _order = value;  // 這行其實沒用到
            

            }
        }
    }
}
