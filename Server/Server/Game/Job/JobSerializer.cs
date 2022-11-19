using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
    public class JobSerializer
    {
		Queue<IJob> _jobQueue = new Queue<IJob>();
		object _lock = new object();
		// 실행중인지 여부
		bool _flush = false;

		// Action도 받을 수 있게 해주는 helper 함수 
		public void Push(Action action) { Push(new Job(action)); }
		public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action,t1)); }
		public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
		public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1,t2,t3)); }

		public void Push(IJob job)
		{
			// 실행할 지 여부
			bool flush = false;

			lock (_lock)
			{
				_jobQueue.Enqueue(job);
				// 처음으로 들어옴 - 실행중X
				if (_flush == false)
					flush = _flush = true;
			}

			// 실행 결정
			if (flush)
				Flush();
		}

		void Flush()
		{
			while (true)
			{
				IJob job = Pop();
				if (job == null)
					return;

				job.Execute();
			}
		}

		IJob Pop()
		{
			lock (_lock)
			{
				// 실행할 일감이 없음
				if (_jobQueue.Count == 0)
				{
					_flush = false;
					return null;
				}
				return _jobQueue.Dequeue();
			}
		}
	}
}
