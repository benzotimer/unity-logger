using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;

namespace Logger
{
    public class FileWriter : IDisposable
    {
        private const string DateFormat = "yyyy-MM-dd";
        private const string LogTimeFormat = "{0:dd/MM/yyyy HH:mm:ss:ffff} [{1}]: {2}\r";
        private const int MaxMessageLenght = 3500;
        
        private readonly string _folder;
        private string _filePath;
        private bool _disposing;
        
        private DateTime _previousDate;

        private FileAppender _fileAppender;

        private readonly Thread _workingThread;
        private readonly Thread _checkNewDateTread;

        private readonly ConcurrentQueue<LogMessage> _messages = new ConcurrentQueue<LogMessage>();
        private readonly ManualResetEvent _manualReset = new ManualResetEvent(true);


        public FileWriter(string folder)
        {
            _folder = folder;
            ManagePath();

            _workingThread = new Thread(StoreMessages)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

            _workingThread.Start();

            _checkNewDateTread = new Thread(CheckNewDate)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

            _checkNewDateTread.Start();
        }

        private void ManagePath()
        {
            _previousDate = DateTime.UtcNow;
            _filePath = $"{_folder}/{DateTime.UtcNow.ToString(DateFormat)}.log";
        }

        public void Write(LogMessage message)
        {
            try
            {
                if (message.Message.Length > MaxMessageLenght)
                {
                    string preview = message.Message.Substring(0, MaxMessageLenght);
                    _messages.Enqueue(new LogMessage(message.Type,
                        $"Message is too long {message.Message.Length}. Preview: {preview}")
                    {
                        Time = message.Time
                    });
                }
                else
                {
                    _messages.Enqueue(message);
                }
            }
            catch (Exception e)
            {
                //
            }
            
            _manualReset.Set();
        }

        private void StoreMessages()
        {
            while (!_disposing)
            {
                while (!_messages.IsEmpty)
                {
                    try
                    {
                        if (!_messages.TryPeek(out LogMessage message))
                        {
                            Thread.Sleep(5);
                        }

                        if (_fileAppender == null || _fileAppender.FileName != _filePath)
                        {
                            _fileAppender = new FileAppender(_filePath);
                        }
                        
                        string messageToWrite = string.Format(LogTimeFormat, message.Time, message.Type, message.Message);

                        if (_fileAppender.Append(messageToWrite))
                        {
                            _messages.TryDequeue(out message);
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }

                _manualReset.Reset();
                _manualReset.WaitOne(500);
            }
        }

        private void CheckNewDate()
        {
            while (!_disposing)
            {
                DateTime currentDate = DateTime.UtcNow;

                if (currentDate.Day != _previousDate.Day)
                {
                    _previousDate = currentDate;
                    ManagePath();
                }

                Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            _disposing = true;
            _workingThread?.Abort();
            _checkNewDateTread?.Abort();
            GC.SuppressFinalize(this);
        }
    }
}