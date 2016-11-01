using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using VKfunctions;

namespace Lottery
{
    class Lot
    {
        public static List<LotteryElement> getPointsList()
        {
            FileStream file = new FileStream("..\\lottery.txt", FileMode.Open);
            StreamReader reader = new StreamReader(file);
            string json = reader.ReadToEnd();
            reader.Close();
            List<LotteryElement> data = JsonConvert.DeserializeObject<List<LotteryElement>>(json);
            return data;
        }

        public static void setPointsList(List<LotteryElement> dataList)
        {
            string serialized = JsonConvert.SerializeObject(dataList, Formatting.Indented);
            FileStream file = new FileStream("..\\lottery.txt", FileMode.Create);
            StreamWriter writer = new StreamWriter(file);
            writer.Write(serialized);
            writer.Close();
        }

        public static void addPointsByID(long ID, int points)
        {
            var dataList = getPointsList();
            try
            {
                dataList.Find(item => item.id == ID).points += points;
                setPointsList(dataList);
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][addPointsByID] " + ex.Message, ConsoleColor.DarkRed);
                dataList.Add(new LotteryElement { id = ID, points = points });
                setPointsList(dataList);
            }
        }
        public static LotteryElement getElementByID(long ID)
        {
            var dataList = getPointsList();
            try
            {
                return dataList.Find(item => item.id == ID);
            }
            catch (Exception ex)
            {
                BotSBS.BODY.debugLine("[VK][addPointsByID] " + ex.Message, ConsoleColor.DarkRed);
                return new LotteryElement { id=0, points=0 };
            }
        }
        public static string getTopElements(int count)
        {
            var dataList = getPointsList();
            dataList.Sort(delegate (LotteryElement x, LotteryElement y)
            {
                return -x.points.CompareTo(y.points);
            });
            string str="Топ пользователей по очкам:\n";
            for (int i = 0; i < count; i++)
            {
                try
                {
                    str += i + 1 + ". " + VK.getFullNameByID(dataList[i].id) + " (" + dataList[i].points + ")" +"\n";
                } catch { }
            }
            return str;
        }
    }
}