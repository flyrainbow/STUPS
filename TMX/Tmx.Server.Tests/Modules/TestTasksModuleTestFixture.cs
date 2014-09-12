﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/22/2014
 * Time: 3:37 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server.Tests.Modules
{
    using System;
    using System.Linq;
    using System.Management.Automation;
	using Gallio.Model.Filters;
    using Nancy;
    using Nancy.Testing;
    using MbUnit.Framework;
    using NUnit.Framework;
	using TMX.Interfaces.Server;
	using Tmx.Client;
	using Tmx.Core;
	using Tmx.Core.Types.Remoting;
	using Tmx.Interfaces;
	using Tmx.Interfaces.Remoting;
	using Tmx.Interfaces.TestStructure;
	using Tmx.Server.Modules;
    using Xunit;
    using PSTestLib;
    
	/// <summary>
	/// Description of TestTasksModuleTestFixture.
	/// </summary>
	[MbUnit.Framework.TestFixture][NUnit.Framework.TestFixture]
	public class TestTasksModuleTestFixture
	{
		public TestTasksModuleTestFixture()
		{
		    TestSettings.PrepareModuleTests();
		}
        
    	[MbUnit.Framework.SetUp][NUnit.Framework.SetUp]
    	public void SetUp()
    	{
    	    TestSettings.PrepareModuleTests();
    	}
    	
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_a_task_to_test_client_if_the_client_matches_the_rule()
        {
        	// Given
            var browser = TestFactory.GetBrowserForTestTasksModule();
			const string testClientHostnameExpected = "testhost";
			const string testClientUsernameExpected = "aaa";
            var testClient = new TestClient { Hostname = testClientHostnameExpected, Username = testClientUsernameExpected };
            var task = new TestTask {
            	Id = 5,
            	Name = "task name",
            	TaskFinished = false,
            	IsActive = true,
            	TaskStatus = TestTaskStatuses.New,
            	Rule = testClientHostnameExpected
            };
			TaskPool.Tasks.Add(task);
            
            // When
            var response = browser.Post(UrnList.TestClientRegistrationPoint, with => with.JsonBody<ITestClient>(testClient));
            testClient = response.Body.DeserializeJson<TestClient>();
            response = browser.Get(UrnList.TestTasks_Root + "/" + testClient.Id);
            var loadedTask = response.Body.DeserializeJson<TestTask>();
            
            // Then
            thenHttpResponseIsOk(response);
            Xunit.Assert.Equal(task.Id, loadedTask.Id);
            Xunit.Assert.Equal(task.Name, loadedTask.Name);
            Xunit.Assert.Equal(task.TaskStatus, loadedTask.TaskStatus);
            Xunit.Assert.Equal(task.TaskFinished, loadedTask.TaskFinished);
            Xunit.Assert.Equal(task.IsActive, loadedTask.IsActive);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_no_task_to_test_client_if_the_client_does_not_match_the_rule()
        {
        	// Given
            var browser = TestFactory.GetBrowserForTestTasksModule();
			const string testClientHostnameExpected = "testhost";
			const string testClientUsernameExpected = "aaa";
            var testClient = new TestClient { Hostname = testClientHostnameExpected, Username = testClientUsernameExpected };
            var task = new TestTask {
            	Id = 5,
            	Name = "task name",
            	TaskFinished = false,
            	IsActive = true,
            	TaskStatus = TestTaskStatuses.New,
            	Rule = "no matches"
            };
			TaskPool.Tasks.Add(task);
            
            // When
            var response = browser.Post(UrnList.TestClientRegistrationPoint, with => with.JsonBody<ITestClient>(testClient));
            testClient = response.Body.DeserializeJson<TestClient>();
            response = browser.Get(UrnList.TestTasks_Root + "/" + testClient.Id);
            
            // Then
            thenHttpResponseIsNotFound(response);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_the_second_task_if_the_client_matches_the_rule()
        {
        	// Given
            var browser = TestFactory.GetBrowserForTestTasksModule();
            var testClient = new TestClient {
                Hostname = "h",
                OsVersion = "w",
                Username = "u"
            };
            var testTask01 = new TestTask {
                Id = 1,
                IsActive = true,
                TaskFinished = false,
                Name = "task name",
                Rule = ".*h.*"
            };
            TaskPool.Tasks.Add(testTask01);
            var testTask02 = new TestTask {
                Id = 2,
                IsActive = true,
                TaskFinished = false,
                Name = "task name 02",
                Rule = "u"
            };
            TaskPool.Tasks.Add(testTask02);
            
            // When
            // imitation
            // ClientSettings.ServerUrl = @"http://localhost:12340";
            // ClientSettings.StopImmediately = false;
            var clientSettings = ClientSettings.Instance;
            clientSettings.ServerUrl = @"http://localhost:12340";
            clientSettings.StopImmediately = false;
            
            // var registration = new Registration();
            var registration = new Registration(new RestRequestCreator());
            
            var response = browser.Post(UrnList.TestClientRegistrationPoint, with => with.JsonBody<ITestClient>(testClient));
            var registeredClient = response.Body.DeserializeJson<TestClient>();
            // imitation
            // ClientSettings.ClientId = registeredClient.Id;
            // ClientSettings.StopImmediately = false;
            clientSettings.ClientId = registeredClient.Id;
            clientSettings.StopImmediately = false;
            
            // var taskLoader = new TaskLoader();
            var taskLoader = new TaskLoader(new RestRequestCreator());
            
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            var task = response.Body.DeserializeJson<TestTask>();
            task.TaskStatus = TestTaskStatuses.Accepted;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            // imitation
            // ClientSettings.CurrentTask = task; // taskLoader.GetCurrentTask();
            // ClientSettings.CurrentTask.TaskResult = new[] { "aaaaa", "bbbbb", "ccccc" };
            // var taskUpdater = new TaskUpdater();
            var taskUpdater = new TaskUpdater(new RestRequestCreator());
            // response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(ClientSettings.CurrentTask));
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            
            task.TaskStatus = TestTaskStatuses.CompletedSuccessfully;
            task.TaskFinished = true;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            task = response.Body.DeserializeJson<TestTask>();
            
            // Then
            thenHttpResponseIsOk(response);
            Xunit.Assert.Equal(testTask02.Id, task.Id);
            Xunit.Assert.Equal(testTask02.IsActive, task.IsActive);
            Xunit.Assert.Equal(testTask02.TaskFinished, task.TaskFinished);
            Xunit.Assert.Equal(testTask02.Name, task.Name);
            // Xunit.Assert.Equal(testTask.Name, clientsetting
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_the_second_task_if_the_client_does_not_match_the_rule()
        {
        	// Given
            var browser = TestFactory.GetBrowserForTestTasksModule();
            var testClient = new TestClient {
                Hostname = "h",
                OsVersion = "w",
                Username = "u"
            };
            var testTask01 = new TestTask {
                Id = 1,
                IsActive = true,
                TaskFinished = false,
                Name = "task name",
                Rule = ".*h.*"
            };
            TaskPool.Tasks.Add(testTask01);
            var testTask02 = new TestTask {
                Id = 2,
                IsActive = true,
                TaskFinished = false,
                Name = "task name 02",
                Rule = "aaa"
            };
            TaskPool.Tasks.Add(testTask02);
            
            // When
            var response = browser.Post(UrnList.TestClientRegistrationPoint, with => with.JsonBody<ITestClient>(testClient));
            var registeredClient = response.Body.DeserializeJson<TestClient>();
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            var task = response.Body.DeserializeJson<TestTask>();
            task.TaskStatus = TestTaskStatuses.Accepted;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            task.TaskStatus = TestTaskStatuses.CompletedSuccessfully;
            task.TaskFinished = true;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            
            // Then
            thenHttpResponseIsNotFound(response);
            // Xunit.Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Xunit.Assert.Equal(testTask.Name, clientsetting
        }
        
    	[MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_the_second_task_if_the_client_matches_the_rule_and_there_are_several()
        {
        	// Given
            var browser = TestFactory.GetBrowserForTestTasksModule();
            var testClient = new TestClient {
                Hostname = "h",
                OsVersion = "w",
                Username = "u"
            };
            var testTask01 = new TestTask {
                Id = 1,
                IsActive = true,
                TaskFinished = false,
                Name = "task name",
                Rule = ".*h.*"
            };
            TaskPool.Tasks.Add(testTask01);
            var testTask02 = new TestTask {
                Id = 2,
                IsActive = true,
                TaskFinished = false,
                Name = "task name",
                Rule = ".*aaa.*"
            };
            TaskPool.Tasks.Add(testTask02);
            var testTask03 = new TestTask {
                Id = 3,
                IsActive = true,
                TaskFinished = false,
                Name = "task name 02",
                Rule = "u"
            };
            TaskPool.Tasks.Add(testTask03);
            var testTask04 = new TestTask {
                Id = 4,
                IsActive = true,
                TaskFinished = false,
                Name = "task name",
                Rule = ".*aaa.*"
            };
            TaskPool.Tasks.Add(testTask04);
            
            // When
            var response = browser.Post(UrnList.TestClientRegistrationPoint, with => with.JsonBody<ITestClient>(testClient));
            var registeredClient = response.Body.DeserializeJson<TestClient>();
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            var task = response.Body.DeserializeJson<TestTask>();
            task.TaskStatus = TestTaskStatuses.Accepted;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            task.TaskStatus = TestTaskStatuses.CompletedSuccessfully;
            task.TaskFinished = true;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            task = response.Body.DeserializeJson<TestTask>();
            
            // Then
            thenHttpResponseIsOk(response);
            Xunit.Assert.Equal(testTask03.Id, task.Id);
            Xunit.Assert.Equal(testTask03.IsActive, task.IsActive);
            Xunit.Assert.Equal(testTask03.TaskFinished, task.TaskFinished);
            Xunit.Assert.Equal(testTask03.Name, task.Name);
            Xunit.Assert.Equal(testTask03.Id, TaskPool.TasksForClients.OrderBy(t => t.Id).Skip(1).First().Id);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_the_second_task_if_the_client_does_not_match_the_rule_and_there_are_several()
        {
        	// Given
            var browser = TestFactory.GetBrowserForTestTasksModule();
            var testClient = new TestClient {
                Hostname = "h",
                OsVersion = "w",
                Username = "u"
            };
            var testTask01 = new TestTask {
                Id = 1,
                IsActive = true,
                TaskFinished = false,
                Name = "task name",
                Rule = ".*h.*"
            };
            TaskPool.Tasks.Add(testTask01);
            var testTask02 = new TestTask {
                Id = 2,
                IsActive = true,
                TaskFinished = false,
                Name = "task name",
                Rule = ".*aaa.*"
            };
            TaskPool.Tasks.Add(testTask02);
            var testTask03 = new TestTask {
                Id = 3,
                IsActive = true,
                TaskFinished = false,
                Name = "task name 02",
                Rule = "aaa"
            };
            TaskPool.Tasks.Add(testTask03);
            var testTask04 = new TestTask {
                Id = 4,
                IsActive = true,
                TaskFinished = false,
                Name = "task name 02",
                Rule = "aaa"
            };
            TaskPool.Tasks.Add(testTask04);
            
            // When
            var response = browser.Post(UrnList.TestClientRegistrationPoint, with => with.JsonBody<ITestClient>(testClient));
            var registeredClient = response.Body.DeserializeJson<TestClient>();
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            var task = response.Body.DeserializeJson<TestTask>();
            task.TaskStatus = TestTaskStatuses.Accepted;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            task.TaskStatus = TestTaskStatuses.CompletedSuccessfully;
            task.TaskFinished = true;
            response = browser.Put(UrnList.TestTasks_Root + "/" + task.Id, with => with.JsonBody<ITestTask>(task));
            response = browser.Get(UrnList.TestTasks_Root + "/" + registeredClient.Id);
            
            // Then
            thenHttpResponseIsNotFound(response);
            // Xunit.Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Xunit.Assert.Equal(testTask.Name, clientsetting
        }
        
        void thenHttpResponseIsOk(BrowserResponse response)
        {
            Xunit.Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        void thenHttpResponseIsNotFound(BrowserResponse response)
        {
            Xunit.Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
	}
}
