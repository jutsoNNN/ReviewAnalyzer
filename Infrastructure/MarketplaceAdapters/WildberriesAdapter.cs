using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ReviewAnalyzer.Core.Entities;
using ReviewAnalyzer.Core.Enums;
using ReviewAnalyzer.Core.Interfaces;
using ReviewAnalyzer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReviewAnalyzer.Infrastructure.MarketplaceAdapters
{
    public class WildberriesAdapter : IMarketplaceAdapter
    {
        public MarketplaceType MarketplaceType => MarketplaceType.Wildberries;

        public async Task<List<ProductReview>> FetchReviewsAsync(string productUrl, int maxReviews)
        {
            var reviews = new List<ProductReview>();
            string productId = ExtractProductId(productUrl);

            try
            {
                Logger.Info("Запуск браузера Chrome для получения HTML");

                var options = new ChromeOptions();
                options.AddArguments(
                    "--headless=new",
                    "--disable-gpu",
                    "--no-sandbox",
                    "--disable-dev-shm-usage",
                    "--disable-blink-features=AutomationControlled",
                    "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                );

                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalOption("useAutomationExtension", false);

                var driverService = ChromeDriverService.CreateDefaultService(".\\chromedriver\\");
                driverService.HideCommandPromptWindow = true;

                using var driver = new ChromeDriver(driverService, options);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(45);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                driver.Navigate().GoToUrl(productUrl);
                Logger.Info($"Перешли на страницу: {productUrl}");

                await Task.Delay(3000);

                for (int i = 0; i < 3; i++)
                {
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight * 0.8);");
                    await Task.Delay(2000);
                }

                try
                {
                    var feedbackTab = driver.FindElement(By.CssSelector("a[href*='feedbacks']"));
                    if (feedbackTab != null)
                    {
                        feedbackTab.Click();
                        await Task.Delay(3000);
                    }
                }
                catch
                {
                    Logger.Warning("Не удалось найти вкладку отзывов");
                }

                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

                try
                {
                    wait.Until(d =>
                        d.FindElements(By.CssSelector("li.comments__item.feedback.product-feedbacks__block-wrapper")).Count > 0);
                }
                catch (WebDriverTimeoutException)
                {
                    Logger.Warning("Истекло время ожидания появления отзывов");
                    return reviews;
                }

                var pageSource = driver.PageSource;
                Logger.Info("HTML получен, начинаем парсинг");
                File.WriteAllText("wildberries_debug.html", pageSource);

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(pageSource);

                var reviewListNode = htmlDoc.DocumentNode
                    .SelectSingleNode("//div[contains(@class, 'user-activity__tab-content')]//ul[contains(@class, 'comments__list')]");

                if (reviewListNode == null)
                {
                    Logger.Warning("Список отзывов не найден");
                    return reviews;
                }

                var reviewNodes = reviewListNode.SelectNodes(".//li[contains(@class, 'comments__item') and contains(@class, 'feedback') and contains(@class, 'product-feedbacks__block-wrapper')]");

                if (reviewNodes == null || reviewNodes.Count == 0)
                {
                    Logger.Warning("Отзывы не найдены в HTML-разметке");
                    return reviews;
                }

                foreach (var node in reviewNodes)
                {
                    if (reviews.Count >= maxReviews) break;

                    var authorNode = node.SelectSingleNode(".//p[contains(@class, 'feedback__header')]");
                    string author = authorNode?.InnerText.Trim() ?? "Неизвестный";

                    var contentNode = node.SelectSingleNode(".//p[contains(@class, 'feedback__text') and contains(@class, 'j-feedback__text')]");
                    string content = contentNode?.InnerText.Trim() ?? "";

                    int rating = 0;
                    var ratingNode = node.SelectSingleNode(".//span[contains(@class, 'feedback__rating')]");
                    if (ratingNode != null)
                    {
                        var classes = ratingNode.GetClasses();
                        if (classes.Contains("star5")) rating = 5;
                        else if (classes.Contains("star4")) rating = 4;
                        else if (classes.Contains("star3")) rating = 3;
                        else if (classes.Contains("star2")) rating = 2;
                        else if (classes.Contains("star1")) rating = 1;
                    }

                    DateTime date = DateTime.Now;
                    var dateNode = node.SelectSingleNode(".//div[contains(@class, 'feedback__date')]");
                    if (dateNode != null)
                    {
                        string dateText = dateNode.InnerText.Trim();
                        date = ParseWildberriesDate(dateText);
                    }

                    reviews.Add(new ProductReview
                    {
                        Content = content,
                        Rating = rating,
                        Source = "Wildberries",
                        Date = date,
                        ProductId = productId
                    });
                }

                Logger.Info($"Успешно получено {reviews.Count} отзывов");
                return reviews;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при парсинге отзывов", ex);
                return reviews;
            }
        }

        public string GetProductId(string productUrl)
        {
            return ExtractProductId(productUrl);
        }

        private string ExtractProductId(string url)
        {
            var match = Regex.Match(url, @"catalog/(\d+)");
            return match.Success ? match.Groups[1].Value : "unknown";
        }

        private DateTime ParseWildberriesDate(string dateText)
        {
            dateText = dateText.ToLowerInvariant().Trim();
            DateTime now = DateTime.Now;

            if (dateText.StartsWith("вчера"))
            {
                var timePart = dateText.Replace("вчера,", "").Trim();
                if (TimeSpan.TryParse(timePart, out var time))
                {
                    return now.Date.AddDays(-1).Add(time);
                }
                return now.AddDays(-1);
            }
            else if (dateText.StartsWith("сегодня"))
            {
                var timePart = dateText.Replace("сегодня,", "").Trim();
                if (TimeSpan.TryParse(timePart, out var time))
                {
                    return now.Date.Add(time);
                }
                return now;
            }
            else
            {
                if (DateTime.TryParse(dateText, out var parsedDate))
                {
                    return parsedDate;
                }
            }

            return now;
        }
    }
}
