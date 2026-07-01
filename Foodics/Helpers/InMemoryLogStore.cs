//using System.Collections.Concurrent;

//namespace Foodics.Helpers
//{
//    public class InMemoryLogStore
//    {
//        private readonly ConcurrentQueue<string> _logs = new();
//        private readonly int _maxLogs = 300; // آخر 300 سجل

//        public void AddLog(string message)
//        {
//            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
//            _logs.Enqueue(logEntry);

//            while (_logs.Count > _maxLogs)
//                _logs.TryDequeue(out _);
//        }

//        public List<string> GetLogs() => _logs.ToList();

//        public void ClearLogs() => _logs.Clear();
//    }
//}
