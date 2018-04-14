using System;
using System.IO;
using System.Collections.Generic;
using SQLite;

using Android.Util;
using Android.Runtime;

namespace nl.pleduc.TsumTsumHeartTracker
{
    [Table("Senders"), Preserve(AllMembers = true)]
    public class TsumTsumSender
    {
        [PrimaryKey]
        public string SenderName { get; set; }

        public int HeartCount { get; set; }

        public DateTime LastReceiveTimestamp { get; set; }

        public override string ToString()
        {
            return $"[SenderName: {SenderName}, HeartCount: {HeartCount}, ReceiveTime: {LastReceiveTimestamp}]";
        }
    }

    public class TsumTsumDatabaseManager
    {
        private const string TAG = "TsumTsumDatabaseManager";
        private string databasePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "senders.sqlite");

        public TsumTsumDatabaseManager()
        {
            if (CreateOrOpenDatabase())
            {
                Log.Info(TAG, "Succesfully created / opened the database!");
            }
        }

        private bool CreateOrOpenDatabase()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(databasePath))
                {
                    connection.CreateTable<TsumTsumSender>();
                    return true;
                }
            }
            catch (SQLiteException exp)
            {
                Log.Error(TAG, $"SQLiteException - CreateOrOpenDatabase, Message: {exp.Message}");
                return false;
            }
        }

        public bool UpdateOrInsertSender(TsumTsumSender sender)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(databasePath))
                {
                    if (ContainsSender(sender.SenderName))
                    {
                        connection.Update(sender);
                    }
                    else
                    {
                        connection.Insert(sender);
                    }

                    Log.Info(TAG, $"Succesfully inserted/replaced {sender} in the database!");

                    return true;
                }
            }
            catch (SQLiteException exp)
            {
                Log.Error(TAG, $"SQLiteException - UpdateOrInsertSender, Message: {exp.Message}");
                return false;
            }
        }

        public bool ContainsSender(string senderName)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(databasePath))
                {
                    List<TsumTsumSender> result = connection.Query<TsumTsumSender>("SELECT * FROM Senders WHERE SenderName = ?", senderName);
                    return result != null && result.Count >= 1;
                }
            }
            catch (SQLiteException exp)
            {
                Log.Error(TAG, $"SQLiteException - ContainsSender, Message: {exp.Message}");
                return false;
            }
        }

        public TsumTsumSender RetrieveOrCreateSenderByName(string senderName)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(databasePath))
                {
                    List<TsumTsumSender> result = connection.Query<TsumTsumSender>("SELECT * FROM Senders WHERE SenderName = ?", senderName);

                    if (result != null && result.Count >= 1)
                    {
                        Log.Info(TAG, $"RetrieveSenderByName, found sender {result[0]}");
                        return result[0];
                    }
                    else
                    {
                        Log.Info(TAG, $"Unable to find sender {senderName}, creating new sender!");
                        return new TsumTsumSender { SenderName = senderName, HeartCount = 0 };
                    }
                }
            }
            catch (SQLiteException exp)
            {
                Log.Error(TAG, $"SQLiteException - RetrieveOrCreateSenderByName, Message: {exp.Message}");
                return null;
            }
        }

        public bool ClearDatabase()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(databasePath))
                {
                    connection.DeleteAll<TsumTsumSender>();
                    return true;
                }
            }
            catch (SQLiteException exp)
            {
                Log.Error(TAG, $"SQLiteException - ClearDatabase, Message: {exp.Message}");
                return false;
            }

        }

        public List<T> PerformRawQuery<T>(string query) where T : new()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(databasePath))
                {
                    return connection.Query<T>(query);
                }
            }
            catch (SQLiteException exp)
            {
                Log.Error(TAG, $"SQLiteException - PerformRawQuery, Message: {exp.Message}");
                return null;
            }
        }

        public TableQuery<TsumTsumSender> RetrieveAllRecords(/*bool ordered, bool ascending*/)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(databasePath))
                {
                    TableQuery<TsumTsumSender> table;

                    table = connection.Table<TsumTsumSender>();

                    Log.Info(TAG, "Succesfully retrieved all records in the database!");
                    return table;
                }
            }
            catch (SQLiteException exp)
            {
                Log.Error(TAG, $"SQLiteException - RetrieveAllRecords, Message: {exp.Message}");
                return null;
            }
        }
    }
}