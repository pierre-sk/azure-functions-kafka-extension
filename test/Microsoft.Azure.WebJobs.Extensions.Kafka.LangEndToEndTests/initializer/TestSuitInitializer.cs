﻿using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.apps.brokers;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.apps.languages;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.apps.type;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.command;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.command.app;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.command.queue;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.executor;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.executor.CommandExecutor;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.helper;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.queue;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.queue.eventhub;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.queue.operation;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.queue.storageQueue;
using Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Azure.WebJobs.Extensions.Kafka.LangEndToEndTests.initializer
{
    // build the eventhub name
    // build the azure storage queue name
    // InitializeTestSuit
    // orchestration of
    // InitializeInfra -- Azure
    // Initilize Kafka broker
    // start app
    public class TestSuitInitializer
    {
        //Intialize Helper vs Constants?
        private InitializeHelper initializeHelper = InitializeHelper.GetInstance();
        private List<Process> processes = new List<Process>();

        //Async
        //Why does this return ICommand?
        public void InitializeTestSuit(Language language, AppType appType, BrokerType brokerType)
        {
            /*var clearStorageQueueTask = ClearStorageQueueAsync(language);
            var createEventHubTask = CreateEventHubAsync(language);
            
            Task.WaitAll(clearStorageQueueTask, createEventHubTask);*/
            Task.WaitAll(StartupApplicationAsync(language, appType, brokerType));
        }

        private async Task StartupApplicationAsync(Language language, AppType appType, BrokerType brokerType)
        {
            Command<Process> command = new ShellCommand.ShellCommandBuilder()
                                            .SetLanguage(language)
                                            .SetAppType(appType)
                                            .SetBrokerType(brokerType)
                                            .Build();
            IExecutor<Command<Process>, Process> executor = new ShellCommandExecutor();
            var process = await executor.ExecuteAsync(command);
            processes.Add(process);
            /*
             * commenting for now for some issues TODO to fix the app issue
             * if(process != null && !process.HasExited)
            {
                return;
            }*/
            // TODO throw excpetion app startup failed
        }

        private async Task ClearStorageQueueAsync(Language language)
        {
            string singleEventStorageQueueName = Utils.BuildStorageQueueName(QueueType.AzureStorageQueue, 
                        AppType.SINGLE_EVENT, language);
            string multiEventStorageQueueName = Utils.BuildStorageQueueName(QueueType.AzureStorageQueue, 
                        AppType.BATCH_EVENT, language);
            
            await ClearStorageQueueAsync(singleEventStorageQueueName, multiEventStorageQueueName);
        }

        private async Task ClearStorageQueueAsync(string singleEventStorageQueueName, string multiEventStorageQueueName)
        {
            Command<QueueResponse> singleCommand = new QueueCommand(QueueType.EventHub,
                        QueueOperation.CREATE, singleEventStorageQueueName);
            Command<QueueResponse> multiCommand = new QueueCommand(QueueType.EventHub,
                        QueueOperation.CREATE, multiEventStorageQueueName);
            
            await Task.WhenAll(singleCommand.ExecuteCommandAsync(), multiCommand.ExecuteCommandAsync());
        }

        private async Task CreateEventHubAsync(Language language)
        {
            string eventHubSingleName = Utils.BuildCloudBrokerName(QueueType.EventHub,
                        AppType.SINGLE_EVENT, language);
            string eventHubMultiName = Utils.BuildCloudBrokerName(QueueType.EventHub,
                        AppType.BATCH_EVENT, language);
            
            await BuildEventHubAsync(eventHubSingleName, eventHubMultiName);
        }

        private async Task BuildEventHubAsync(string eventhubNameSingleEvent, string eventhubNameMultiEvent) 
        {
            Command<QueueResponse> singleCommand = new QueueCommand(QueueType.EventHub, 
                        QueueOperation.CREATE, eventhubNameSingleEvent);
            Command<QueueResponse> multiCommand = new QueueCommand(QueueType.EventHub, 
                        QueueOperation.CREATE, eventhubNameMultiEvent);
            
            await Task.WhenAll(singleCommand.ExecuteCommandAsync(), multiCommand.ExecuteCommandAsync());

        }
    }
}
