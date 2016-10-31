using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TMfunctions
{
    class TM
    {
        public static async void getNewMessages()
        {
            var offset = 0;
            while (true)
            {
                try
                {
                    await Task.Delay(1000);

                    FileStream file1 = new FileStream("..\\auth.txt", FileMode.Open); //создаем файловый поток
                    StreamReader reader = new StreamReader(file1); // создаем «потоковый читатель» и связываем его с файловым потоком 
                    string json = reader.ReadToEnd(); //считываем все данные с потока и выводим на экран
                    reader.Close(); //закрываем поток
                    AuthInfo Auth = JsonConvert.DeserializeObject<AuthInfo>(json);

                    var Bot = new Telegram.Bot.TelegramBotClient(Auth.tmToken);
                    var updinf = await Bot.GetUpdatesAsync(offset);
                    foreach (var upd in updinf)
                    {
                        BotSBS.BODY.debugLine("[TM] " + upd.Message.From.FirstName + " " + upd.Message.From.LastName + " (" + upd.Message.From.Username + "): " + upd.Message.Text, ConsoleColor.White);
                        if (upd.Message.Text != null)
                        {
                            switch (upd.Message.Text)
                            {
                                case "/week": sendSchedule(upd.Message.Chat.Id, "bothWeeks", false); break;
                                case "/week@sbs501inf_bot": sendSchedule(upd.Message.Chat.Id, "bothWeeks", false); break;
                                case "/evenweek": sendSchedule(upd.Message.Chat.Id, "evenWeek", false); break;
                                case "/evenweek@sbs501inf_bot": sendSchedule(upd.Message.Chat.Id, "evenWeek", false); break;
                                case "/oddweek": sendSchedule(upd.Message.Chat.Id, "oddWeek", false); break;
                                case "/oddweek@sbs501inf_bot": sendSchedule(upd.Message.Chat.Id, "oddWeek", false); break;
                                case "/tomorrow": sendSchedule(upd.Message.Chat.Id, "tomorrow", false); break;
                                case "/tomorrow@sbs501inf_bot": sendSchedule(upd.Message.Chat.Id, "tomorrow", false); break;
                                case "/today": sendSchedule(upd.Message.Chat.Id, "today", false); break;
                                case "/today@sbs501inf_bot": sendSchedule(upd.Message.Chat.Id, "today", false); break;
                                default: break;
                            }
                        }
                        offset = upd.Id + 1;
                    }
                    BotSBS.BODY.UpdateConsoleName();
                }
                catch (Exception ex)
                {
                    BotSBS.BODY.debugLine("[TM][getNewMessagesTM] " + ex.Message, ConsoleColor.DarkRed);
                    Thread.Sleep(15000);
                }
            }
        }
        public static async void sendSchedule(long chatId, string type, bool weatherState)
        {
            try
            {
                var weather = "";
                if (weatherState) { weather = "Погода \n в 08:00: " + BotSBS.BODY.getWeatherData("2h") + "\n в 13:00: " + BotSBS.BODY.getWeatherData("7h") + "\n\n"; }
                var Bot = new Telegram.Bot.TelegramBotClient("253978743:AAFvRtCT02mnnEg0aEv5tSE__JP6S85cuPU");
                await Bot.SendTextMessageAsync(chatId, weather + BotSBS.BODY.getSchedule(type));
                BotSBS.BODY.debugLine("[TM] Schedule with type " + type + " has been sent.", ConsoleColor.DarkGreen);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[TM][sendScheduleTM] " + ex.Message, ConsoleColor.DarkRed);
                await Task.Delay(5000);
            }

        }
    }
}