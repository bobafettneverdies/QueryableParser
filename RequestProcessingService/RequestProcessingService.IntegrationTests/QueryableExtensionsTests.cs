using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RequestProcessingService.IntegrationTests.TestObjects;

namespace RequestProcessingService.IntegrationTests
{
    [TestFixture]
    public class QueryableExtensionsTests
    {
        private IQueryable<TestDTO> _testQuery;

        public QueryableExtensionsTests()
        {
            var list = new List<TestDTO>();

            for (int i = 0; i < 200; i++)
            {
                list.Add(new TestDTO(i));
            }

            _testQuery = list.AsQueryable();
        }

        [Test]
        public void ProcessGridRequest_Test()
        {
            var testRequest = new TestTypeaheadRequest
            {
                Filter = new GridFilter()
                {
                    Logic = "And",
                    Filters = new List<GridFilter>()
                    {
                        new GridFilter
                        {
                            Operator = "Gte",
                            Value = "164", 
                            Field = "id"
                        },
                        new GridFilter()
                        {
                            Operator = "EndsWith",
                            Value = "9",
                            Field = "Country"
                        }
                    }
                },
                Page = 1,
                Take = 20,
                Sort = new List<Sort> { new Sort { Dir = "desc", Field = "id" }, new Sort { Dir = "asc", Field = "City" } }
            };

            var response = _testQuery.ProcessGridRequest(testRequest);

            Assert.IsNotEmpty(response.Data as List<TestDTO>);
        }

        [Test]
        public void ProcessTypeaheadRequest_Test()
        {
            var testRequest = new TestTypeaheadRequest
            {
                Filter = new TypeaheadFilter()
                {
                    Value = "123",
                    Fields = new List<string>{ "Country", "City", "House" }
                },
                Page = 1,
                Take = 2,
                Sort = new List<Sort> { new Sort() {Dir = "desc", Field = "id"} }
            };

            var response = _testQuery.ProcessTypeaheadRequest(testRequest);
            
            Assert.IsNotEmpty(response.Data as List<TestDTO>);
        }
    }
}
