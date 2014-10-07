﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/13/2014
 * Time: 2:25 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server.Modules
{
    using System;
	using System.Text;
    using System.Linq;
	using System.Xml.Linq;
    using Nancy;
    using Nancy.ModelBinding;
	using Tmx.Interfaces;
	using Tmx.Interfaces.Server;
	using Tmx;
	using Tmx.Interfaces.TestStructure;
    
    /// <summary>
    /// Description of TestResultsModule.
    /// </summary>
    public class TestResultsModule : NancyModule
    {
        public TestResultsModule() : base(UrnList.TestStructure_Root)
        {
            StaticConfiguration.DisableErrorTraces = false;
            
            Post[UrnList.TestStructure_AllResults] = _ => importTestResults();
            
            Post[UrnList.TestStructure_Suites] = _ => {
                var testSuite = this.Bind<TestSuite>();
                TmxHelper.NewTestSuite(testSuite.Name, testSuite.Id, testSuite.PlatformId, testSuite.Description, testSuite.BeforeScenario, testSuite.AfterScenario);
                TestData.SetSuiteStatus(true);
				return TmxHelper.OpenTestSuite(testSuite.Name, testSuite.Id, testSuite.PlatformId) ? HttpStatusCode.Created : HttpStatusCode.InternalServerError;
            };
        	
        	Post[UrnList.TestStructure_Scenarios] = _ => {
                // var testScenario = this.Bind<TestScenario>();
                var testScenario = this.Bind<TestScenario>("DbId", "TestResults", "Timestamp", "BeforeTest", "AfterTest", "BeforeTestParameters", "AfterTestParameters", "TestCases", "TimeSpent", "Statistics", "enStatus");
                
        		var dataObjectAdd = new AddScenarioCmdletBaseDataObject {
					AfterTest = testScenario.AfterTest,
					BeforeTest = testScenario.BeforeTest,
					Description = testScenario.Description,
					Id = testScenario.Id,
        			Name = testScenario.Name,
        			TestPlatformId = testScenario.PlatformId,
        			TestSuiteId = testScenario.SuiteId
        		};
        		TmxHelper.AddTestScenario(dataObjectAdd);
        		TestData.SetScenarioStatus(true);
        		
        		var dataObjectOpen = new OpenScenarioCmdletBaseDataObject {
        			Name = testScenario.Name,
        			Id = testScenario.Id,
        			TestPlatformId = testScenario.PlatformId
        		};
//        		return TmxHelper.OpenTestScenario(dataObjectOpen) ? HttpStatusCode.Created : HttpStatusCode.InternalServerError;
        		
        		return HttpStatusCode.Created;
        	};
            
        	Post[UrnList.TestStructure_Results] = _ => {
ITestResult testResult = null;
try {
	testResult = this.Bind<TestResult>(); // "DbId", "TestResults", "Timestamp", "BeforeTest", "AfterTest", "BeforeTestParameters", "AfterTestParameters", "TestCases", "TimeSpent", "Statistics", "enStatus");
        		
}
catch (Exception eeee) {
	Console.WriteLine(eeee.Message);
}

//        		var dataObjectAdd = new AddScenarioCmdletBaseDataObject {
//					AfterTest = testResult.AfterTest,
//					BeforeTest = testResult.BeforeTest,
//					Description = testResult.Description,
//					Id = testResult.Id,
//        			Name = testResult.Name,
//        			TestPlatformId = testResult.PlatformId,
//        			TestSuiteId = testResult.SuiteId
//        		};
//        		TmxHelper.AddTestScenario(dataObjectAdd);
//        		TestData.SetScenarioStatus(true);
//        		
//        		var dataObjectOpen = new OpenScenarioCmdletBaseDataObject {
//        			Name = testResult.Name,
//        			Id = testResult.Id,
//        			TestPlatformId = testResult.PlatformId
//        		};
//        		return TmxHelper.OpenTestScenario(dataObjectOpen) ? HttpStatusCode.Created : HttpStatusCode.InternalServerError;
//        		
//var result = TmxHelper.OpenTestScenario(dataObjectOpen);
//Console.WriteLine(result);
        		return HttpStatusCode.Created;
        	};
        }

        HttpStatusCode importTestResults()
        {
            try {
                var actualBytes = new byte[Request.Body.Length];
                Request.Body.Read(actualBytes, 0, (int)Request.Body.Length);
                var actual = Encoding.UTF8.GetString(actualBytes);
                var xDoc = XDocument.Parse(actual);
                TmxHelper.ImportTestResultsFromXdocument(xDoc);
                // maybe, there's no such need? // TODO: set current test suite, test scenario, test result?
                return HttpStatusCode.Created;
            } catch (Exception) {
                return HttpStatusCode.ExpectationFailed;
            }
        }
    }
}
