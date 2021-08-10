#if USE_SQLITEPCL_RAW
using System;
using System.Linq;
using NUnit.Framework;

namespace SQLite.Tests
{
	[TestFixture]
	public class CreateFunctionTests
	{
		public class State
		{
			public int TotalNumber { get; set; }
		}
		
		[Test]
		public void Create_Function_No_Args ()
		{
			var datetime = DateTime.Now;
			using (var conn = new TestDb ()) {
				conn.RegisterFunction<DateTime> ("MyFunc", null, state => datetime);
				var result = conn.ExecuteScalar<DateTime> ("Select MyFunc()");

				Assert.AreEqual(datetime, result);
			}
		}

		[Test]
		public void Create_Function_With_An_Arg ()
		{
			using (var conn = new TestDb ()) {
				conn.RegisterFunction<int, string> ("MyFunc", null, (num, state) => num.ToString("000,000"));
				var str = conn.ExecuteScalar<string> ("Select MyFunc(1)");

				Assert.AreEqual("000,001", str);
			}
		}
		
		[Test]
		public void Create_Function_With_2_Args ()
		{
			using (var conn = new TestDb ()) {
				conn.RegisterFunction<int, double, string> (
					"MyFunc", 
					null, 
					(num, dobl, state) => (num * dobl).ToString("000,000"));
				var str = conn.ExecuteScalar<string> ("Select MyFunc(1, 1.10025)");

				Assert.AreEqual((1 * 1.10025).ToString("000,000"), str);
			}
		}

		[Test]
		public void Create_Aggrgate_Function_With_An_Arg ()
		{
			using (var conn = new TestDb ()) {
				conn.RegisterAggregateFunction<int, int> ("MyFunc", new State (),
					(num, state) => {
						((State)state).TotalNumber += num;
					},
					state => {
						var result = ((State)state).TotalNumber;
						((State)state).TotalNumber = 0;
						return result;
					});

				conn.CreateTable<Product> ();
				
				conn.InsertAll (
					from i in Enumerable.Range(1, 20) 
					select new Product {
						Name = "#" + i,
						TotalSales = 0,
						Price = i,
					});

				var result = conn.ExecuteScalar<int> ("Select MyFunc(Id) From Product");

				Assert.AreEqual(result, Enumerable.Range(1, 20).Sum());
			}
		}
	}
}
#endif
