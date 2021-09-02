using System;
using System.Diagnostics;
using System.Threading;
using ASF.Core.Common.Cache;
using ASF.Core.Common.Consts;
using ASF.Core.Common.Helpers;
using ASF.Core.Common.Helpers.Interfaces;
using ASF.Core.Common.SecretsManager;
using ASF.Client.MS.PushConsumer.Helpers;
using Confluent.Kafka;
using MongoDB.Driver;
using log4net;
using PushTechnology.ClientInterface.Client.Session;

/// <summary>
/// Push Consumer
/// 
/// Expected Environmental variables:
///  Environment - "dev"
///  TransmissionAuditing - "true"
///  Debugging - "true"
///  PushInstance - "master"
///  PushUser - "publisher"
/// 
/// </summary>
namespace ASF.Client.MS.PushConsumer
{
    public class Program
    {
        private static ILog _logger;
        private static SecretsManagerClient _secretsManager = new SecretsManagerClient();
        private static ISerializer _serializer = new JsonSerializer();
        private static MemoryCache _memoryCache;
        private static IProducer<Null, string> _producer;
        private static string _pushConnectionString;
        private static ISession _pushSession;
        
        public static void Main(string[] args)
        {
            Console.WriteLine($"Push Consumer: {Process.GetCurrentProcess().Id}\n");

            //get environment vars
            var (environment, transmissionAuditing, debugging, pushInstance, pushUser) = GetEnvironmentalVariables();

            //configure logger
            //Logger.ConfigureLog4net(environment, Common.Enums.MicroServiceType.ApiPublisher);
            _logger = LogManager.GetLogger(typeof(Program));
            _logger.Info("Push Consumer Starting");


            CancellationTokenSource cts = new();

            #region connection details/sessions

            #region redis

            var redisConnectionString = _secretsManager.GetRedisConnectionString(environment, true);

            //open connection
            _memoryCache = new MemoryCache(redisConnectionString);


            #endregion

            #region push

            _pushConnectionString = _secretsManager.GetPushConnectionString(environment, pushInstance, pushUser);

            //open connection
            _pushSession = new ConnectionHelper().OpenConnection(_pushConnectionString);

            #endregion

            #region kafka

            //get kafka broker details
            var bootstrapServers = _secretsManager.GetKafkaBootstrapServers(environment);

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                ClientId = "push-consumer",
            };

            //open connection
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();

            #endregion

            #endregion

            TopicHelper topicHelper = new TopicHelper(_memoryCache, _producer);
            //topicHelper.AddPTStream(_pushSession, Topics.ADJUSTMENT_POC);
            //topicHelper.SubscribeTopic(_pushSession, Topics.ADJUSTMENT_POC);

            try
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        //receive message
                        //var consumeResult = kafkaConsumer.Consume(cts.Token);

                        //msgConsumer.Consume(consumeResult);
                    }
                    catch (ConsumeException e)
                    {
                        _logger.Error(e);
                        Console.WriteLine($"Error occured: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Close and Release all the resources held by this consumer  
                //kafkaConsumer.Close();
            }
        }
    

        private static (string, bool, bool, string, string) GetEnvironmentalVariables()
        {
            //standard environmental variables
            var (environment, transmissionAuditing, debugging, useAWSDebugProfile, AWSDebugSecret, AWSDebugAccessKey) = EnvironmentHelper.GetEnvironmentalVariables();
            _secretsManager.setAWSDebugProfile(useAWSDebugProfile, AWSDebugAccessKey, AWSDebugSecret);

            //microservice specific environmental variables
            var pushInstance = Environment.GetEnvironmentVariable("PushInstance");
            var pushUser = Environment.GetEnvironmentVariable("PushUser");
            
            return (environment, transmissionAuditing, debugging, pushInstance, pushUser);
        }
    }
}
