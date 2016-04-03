using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestProcessingService.IntegrationTests.TestObjects
{
    public class TestDTO
    {
        public TestDTO(int index)
        {
            id = index;
            Country = "Страна" + index;
            City = "Город" + (index + 1);
            House = "Дом" + (index + 2);
        }

        public int id { get; private set; }

        public String Country { get; private set; }

        public String City { get; private set; }

        public String House { get; private set; }
    }
}
