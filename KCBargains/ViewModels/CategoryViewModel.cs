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

        public CategoryViewModel() { }

        public CategoryViewModel(int id, string name) 
        {
            Id = id;
            Name = name;

            ArrayList productNames = new ArrayList();
            ProductNames = productNames;
        }
    }
}
