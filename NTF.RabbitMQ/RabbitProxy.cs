using NTF.Logger;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTF.RabbitMQ
{
    public class RabbitProxy
    {
        public ILogger Logger { get; set; }

        private readonly string url;
        /// <summary>
        /// 发布消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="model">消息内容</param>
        /// <returns>是否发布成功</returns>
        public bool Publish<T>(string queueName, T model)
        {
            if (string.IsNullOrEmpty(url))
            {
                Logger.Error("请在AppSettings中配置MQ节点RabbitMQ");
                return false;
            }
            if (model == null)
            {
                return false;
            }
            //序列化对象
            var msg = model.ToJson();
            try
            {
                var factory = new ConnectionFactory();
                factory.Uri = url;
                using (var conn = factory.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        //在MQ上定义一个持久化队列，如果名称相同不会重复创建
                        channel.QueueDeclare(queueName, true, false, false, null);

                        byte[] bytes = Encoding.UTF8.GetBytes(msg);

                        //设置消息持久化
                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 2;

                        //发送消息到队列
                        channel.BasicPublish("", queueName, properties, bytes);

                        //logger.Info("发送至队列:" + queueName + ":" + msg);
                        return true;
                    }
                }
            }
            catch (Exception e1)
            {
                Logger.Error("入列失败：" + queueName + ":" + msg + e1.Message);
                return false;
            }
        }
        /// <summary>
        /// 发布消息（批量发布）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool BatchPublish<T>(string queueName, IList<T> list)
        {
            if (string.IsNullOrEmpty(url))
            {
                Logger.Error("请在AppSettings中配置MQ节点RabbitMQ或者通过构造函数传入MQ连接字符串");
                return false;
            }
            if (list == null || list.Count() == 0)
            {
                return false;
            }
            try
            {
                var factory = new ConnectionFactory();
                factory.Uri = url;
                using (var conn = factory.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        channel.QueueDeclare(queueName, true, false, false, null);
                        //设置消息持久化
                        IBasicProperties properties = channel.CreateBasicProperties();
                        properties.DeliveryMode = 2;
                        foreach (var item in list)
                        {
                            var msg = item.ToJson();
                            //在MQ上定义一个持久化队列，如果名称相同不会重复创建
                            byte[] bytes = Encoding.UTF8.GetBytes(msg);
                            //发送消息到队列
                            channel.BasicPublish("", queueName, properties, bytes);
                        }
                        return true;
                    }
                }
            }
            catch (Exception e1)
            {
                Logger.Error("入列失败：" + queueName + ":" + e1.Message);
                return false;
            }
        }
        /// <summary>
        /// 消息订阅
        /// </summary>
        /// <![CDATA[
        ///RabbitMQHelper.Subscribe("test1",
        ///                    msg=>{
        ///                     //TODO
        ///                     return true;
        ///                    });
        /// ]]>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="func">接收到消息后执行的操作</param>
        public void Subscribe<T>(string queueName, Func<T, bool> func)
        {
            var isClose = false;//队列服务端是否关闭
            do
            {
                try
                {
                    var factory = new ConnectionFactory();
                    factory.Uri = url;
                    using (var conn = factory.CreateConnection())
                    {
                        using (var channel = conn.CreateModel())
                        {
                            //在MQ上定义一个持久化队列，如果名称相同不会重复创建
                            channel.QueueDeclare(queueName, true, false, false, null);

                            //在队列上定义一个消费者
                            var consumer = new QueueingBasicConsumer(channel);
                            //消费队列，并设置应答模式为程序主动应答
                            channel.BasicConsume(queueName, false, consumer);

                            var closeErrorCount = 0;//队列服务关闭重试次数

                            while (true)
                            {
                                var msg = "";
                                try
                                {
                                    if (channel.IsClosed)
                                    {
                                        isClose = true;
                                        break;
                                    }
                                    //阻塞函数，获取队列中的消息
                                    var ea = consumer.Queue.Dequeue();

                                    byte[] bytes = ea.Body;
                                    msg = Encoding.UTF8.GetString(bytes);
                                    var instance = msg.FromJson<T>();
                                    var success = func(instance);
                                    //应答确认
                                    if (success)
                                    {
                                        channel.BasicAck(ea.DeliveryTag, false);
                                        if (closeErrorCount > 0)
                                            closeErrorCount = 0;
                                    }
                                    else
                                    {
                                        Logger.Error("消息未确认：" + queueName + ":" + msg);
                                    }
                                }
                                catch (System.IO.EndOfStreamException closeEx)//捕获队列服务端关闭的异常
                                {
                                    Logger.Error("队列服务端挂了，联系管理员吧：" + queueName + ":" + msg + "closeEx:" + closeEx);
                                    isClose = true;
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Logger.Error("队列异常，联系管理员吧：" + queueName + ":" + msg + "ex:" + ex);
                                    isClose = true;
                                    break;
                                }
                            }

                        }
                    }
                }
                catch (Exception exception)
                {
                    Logger.Error("队列异常，联系管理员吧：" + queueName + ":" + "closeEx:" + exception);
                    isClose = true;
                }
                System.Threading.Thread.Sleep(2 * 60 * 1000);//休息 5 分钟重新连接队列服务端；
            } while (isClose);
        }
    }
}
