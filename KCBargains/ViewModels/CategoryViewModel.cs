using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCBargains.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ArrayList ProductNames { get; set; }
        public string AdminEmail { get; set; }
        public string TimeLog { get; set; }
        public CategoryViewModel() { }

        public CategoryViewModel(int id, string name, string adminEmail, string timeLog) 
        {
            Id = id;
            Name = name;
            AdminEmail = adminEmail;
            TimeLog = timeLog;

            ArrayList productNames = new ArrayList();
            ProductNames = productNames;
        }
    }
}
