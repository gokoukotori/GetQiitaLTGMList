using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;

namespace GetQiitaLTGMList
{
	/// <summary>
	/// LGTM一覧からURL抽出するやつ
	/// </summary>
	class Program
	{
		/// <summary>
		/// None
		/// </summary>
		/// <param name="args">ユーザーID</param>
		static void Main(string[] args)
		{
			var chromeOptions = new ChromeOptions
			{
				PageLoadStrategy = PageLoadStrategy.Normal
			};

			var urlList = new List<string>();

			using IWebDriver driver = new ChromeDriver(chromeOptions);
			try
			{
				driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
				driver.Navigate().GoToUrl("https://qiita.com/" + args[0] + "/lgtms");
				WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
				wait.IgnoreExceptionTypes(typeof(NoSuchElementException));
				var firstResult = wait.Until(e => e.FindElement(By.ClassName("st-Pager_count")).FindElement(By.TagName("span")));
				var maxPage = int.Parse(firstResult.Text.Split("/")[1].Trim());
				for (int i = 0; i < maxPage; i++)
				{
					if (i != 0)
						driver.Url = "https://qiita.com/" + args[0] + "/lgtms?page=" + (i + 1);
					var fwait = new WebDriverWait(driver, TimeSpan.FromSeconds(30))
					{
						PollingInterval = TimeSpan.FromSeconds(5),
					};
					fwait.IgnoreExceptionTypes(typeof(NoSuchElementException));
					var result = fwait.Until(e => e.FindElement(By.CssSelector(".ItemListArticleWithAvatar__Item-sc-1jj9c6g-0.dzFkMp")));
					var urlEls = driver.FindElements(By.CssSelector(".ItemListArticleWithAvatar__Item-sc-1jj9c6g-0.dzFkMp")).Select(x => x.FindElements(By.TagName("a"))[x.FindElements(By.TagName("a")).Count - 2]);
					var urls = urlEls.Select(x => x.GetAttribute("href"));
					urlList.AddRange(urls);
				}

			}
			catch (Exception ex)
			{
				driver.Quit();
				driver.Dispose();
				return;
			}
			finally
			{
				driver.Quit();
				driver.Dispose();
			}

			var directory = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "OUTPUT");

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			if (File.Exists(Path.Combine(directory, "output.txt")))
			{
				File.Move(Path.Combine(directory, "output.txt"), Path.Combine(directory, "output_" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".txt"));
			}
			StreamWriter sw = new StreamWriter(Path.Combine(directory, "output.txt"), false, Encoding.UTF8);
			foreach (var item in urlList)
			{
				sw.Write(item + sw.NewLine);
			}
			sw.Close();
			sw.Dispose();
		}
	}
}