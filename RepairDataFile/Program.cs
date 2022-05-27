using System;
using System.Collections.Generic;
using System.IO;

namespace RepairDataFile
{
    class Program
    {
        
        static void Main(string[] args)
        {
            List<messageSet> dataset;
            if (args.Length > 0 && File.Exists(args[0]))
            {
                int result = RepairFile.modulusCheck(args[0]);
                if (result == 0)
                {
                    dataset = RepairFile.loadMessageset(args[0]);
                }else
                {
                    switch (result)
                    {
                        case 7:
                            Console.WriteLine("The data files size was inconsistant mod by 14");
                            break;
                        case 8:
                            Console.WriteLine("The data files message set integrity check was wrong.");
                            
                            break;
                        default:
                            Console.WriteLine("result = " + result.ToString());
                            break;
                    }

                    RepairFile.repair(args[0]);
                }
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }


    public static class RepairFile
    {
        


        /// <summary>
        /// Finds the file size in bytes then runs a mod 14 to see if the file is the right size.
        /// also checks to see if all messages are intact.
        /// </summary>
        /// <param name="sFileName"></param>
        public static int modulusCheck(string sFileName)
        {
            

            FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            long fSize = fs.Length;

            long chk = fSize % 14;
            if (chk != 0)
            {
                reader.Close();
                fs.Close();
                return 7;
            }
            long allMessagesPresent = fSize % (14 * 11);

            if (allMessagesPresent != 0)
            {
                reader.Close();
                fs.Close();
                return 8;
            }

            Console.WriteLine("Running check on file " + sFileName);
            Console.WriteLine(fSize.ToString() + " bytes");
            Console.WriteLine(chk.ToString() + " modulus check");
            Console.WriteLine(allMessagesPresent.ToString() + " Message integrity check");

            reader.Close();
            fs.Close();
            return 0;
        }

        public static void repair(string sFileName)
        {
            FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            long bytesToremove = fs.Length % (14 * 11);

            Console.WriteLine("File Name: " + sFileName);
            Console.WriteLine("File Size: " + fs.Length.ToString() + " bytes");
            Console.WriteLine("Extra bytes: " + bytesToremove.ToString() + " bytes");
            
            long newLoops = fs.Length - bytesToremove;
            Console.WriteLine("New File size: " + newLoops.ToString() + " bytes");
            List<message> msgList = new List<message>();


            for (int i = 0; i < newLoops / 14; i++)
            {
                message singleMessage = new message();

                UInt32 temptime = reader.ReadUInt32();
                UInt16 tempid = reader.ReadUInt16();
                byte tempf0 = reader.ReadByte();
                byte tempf1 = reader.ReadByte();
                byte tempf2 = reader.ReadByte();
                byte tempf3 = reader.ReadByte();
                byte tempf4 = reader.ReadByte();
                byte tempf5 = reader.ReadByte();
                byte tempf6 = reader.ReadByte();
                byte tempf7 = reader.ReadByte();

                singleMessage.setMessageTime(temptime);
                singleMessage.setMessageId(tempid);
                singleMessage.setf0(tempf0);
                singleMessage.setf1(tempf1);
                singleMessage.setf2(tempf2);
                singleMessage.setf3(tempf3);
                singleMessage.setf4(tempf4);
                singleMessage.setf5(tempf5);
                singleMessage.setf6(tempf6);
                singleMessage.setf7(tempf7);

                msgList.Add(singleMessage);

            }




            reader.Close();
            fs.Close();

            writeNewRepairedFile(msgList, sFileName);
        }


        /// <summary>
        /// Only use this function if data is missing on the end of a file
        /// </summary>
        /// <param name="newFile"></param>
        /// <param name="sFileName"></param>
        /// <returns></returns>
        public static string writeNewRepairedFile(List<message> newFile, string sFileName)
        {
            if (File.Exists(sFileName))
            {
                File.Copy(sFileName, sFileName + ".backup");
                File.Delete(sFileName);
                Console.WriteLine("File was backed up to " + sFileName + ".backup");
            }

            FileStream fs = new FileStream(sFileName, FileMode.Create);

            BinaryWriter writer = new BinaryWriter(fs);

            for (int i = 0; i < newFile.Count; i++)
            {
                UInt32 temptime = newFile[i].m_msgtime;
                UInt16 tempid = newFile[i].m_msgid;
                byte tempf0 = newFile[i].m_f0;
                byte tempf1 = newFile[i].m_f1;
                byte tempf2 = newFile[i].m_f2;
                byte tempf3 = newFile[i].m_f3;
                byte tempf4 = newFile[i].m_f4;
                byte tempf5 = newFile[i].m_f5;
                byte tempf6 = newFile[i].m_f6;
                byte tempf7 = newFile[i].m_f7;


                writer.Write(temptime);
                writer.Write(tempid);
                writer.Write(tempf0);
                writer.Write(tempf1);
                writer.Write(tempf2);
                writer.Write(tempf3);
                writer.Write(tempf4);
                writer.Write(tempf5);
                writer.Write(tempf6);
                writer.Write(tempf7);

            }


            writer.Close();
            fs.Close();

            return "Success";
        }

        public static List<messageSet> loadMessageset(string sFileName)
        {
            List<messageSet> msgList = new List<messageSet>();

            FileStream fs = new FileStream(sFileName, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            // Calculate how many records there are
            long loops = fs.Length / 14;
           
            for (int i = 0; i < (loops / 11); i++)
            {
                messageSet set = new messageSet();
                for (int j = 0; j < 11; j++)
                {
                    

                    UInt32 temptime = reader.ReadUInt32();
                    UInt16 tempid = reader.ReadUInt16();
                    byte tempf0 = reader.ReadByte();
                    byte tempf1 = reader.ReadByte();
                    byte tempf2 = reader.ReadByte();
                    byte tempf3 = reader.ReadByte();
                    byte tempf4 = reader.ReadByte();
                    byte tempf5 = reader.ReadByte();
                    byte tempf6 = reader.ReadByte();
                    byte tempf7 = reader.ReadByte();

                    set.loadMessage(temptime, tempid, tempf0, tempf1, tempf2, tempf3, tempf4, tempf5, tempf6, tempf7);

                    
                }
                
                if (set.isComplete)
                {
                    
                    msgList.Add(set);

                }else
                {
                    string[] errorFile = new string[2];
                    errorFile[0] = "File had errors on record " + (i * 11).ToString();
                    if (File.Exists("error.txt"))
                    {
                        File.Delete("error.txt");
                    }
                    File.WriteAllLines("error.txt", errorFile);
                    Console.WriteLine("File had errors, wrote description to error.txt");
                    return null;
                }
            }

            reader.Close();
            fs.Close();


            return msgList;
        }


    }


    public class message
    {
        // Arrays to hold all the message data
        public UInt32 m_msgtime = 0;
        public UInt16 m_msgid = 0;
        public byte m_f0 = 0;
        public byte m_f1 = 0;
        public byte m_f2 = 0;
        public byte m_f3 = 0;
        public byte m_f4 = 0;
        public byte m_f5 = 0;
        public byte m_f6 = 0;
        public byte m_f7 = 0;
        public message()
        {

        }
        public void setMessageTime(UInt32 newTime)
        {
            this.m_msgtime = newTime;
        }

        public void setMessageId(UInt16 newId)
        {
            this.m_msgid = newId;
        }

        public void setf0(byte nf0)
        {
            this.m_f0 = nf0;
        }
        public void setf1(byte nf1)
        {
            this.m_f1 = nf1;
        }
        public void setf2(byte nf2)
        {
            this.m_f2 = nf2;
        }
        public void setf3(byte nf3)
        {
            this.m_f3 = nf3;
        }
        public void setf4(byte nf4)
        {
            this.m_f4 = nf4;
        }
        public void setf5(byte nf5)
        {
            this.m_f5 = nf5;
        }
        public void setf6(byte nf6)
        {
            this.m_f6 = nf6;
        }
        public void setf7(byte nf7)
        {
            this.m_f7 = nf7;
        }
    }

    public class messageSet
    {
        public enum testResults
        {
            nottested,
            pass,
            badByteCheck,
            badMessageCheck
        }
        public message[] m_set;

        public int[] msgIDs;

        private bool[] filled = { false, false, false, false, false, false, false, false, false, false, false };

        public bool isComplete = false;
        
        public messageSet()
        {
            this.m_set = new message[11];

            for (int i = 0; i < 11; i++)
            {
                this.m_set[i] = new message();
            }
            

            this.msgIDs = new int[11];
            msgIDs[0] = 100;
            msgIDs[1] = 200;
            msgIDs[2] = 201;
            msgIDs[3] = 202;
            msgIDs[4] = 203;
            msgIDs[5] = 204;
            msgIDs[6] = 205;
            msgIDs[7] = 206;
            msgIDs[8] = 207;
            msgIDs[9] = 208;
            msgIDs[10] = 500;
        }

        public void loadMessage(UInt32 m_time, UInt16 mID, byte m_f0, byte m_f1, byte m_f2, byte m_f3, byte m_f4, byte m_f5, byte m_f6, byte m_f7)
        {
            
                int currindex = -1;
                if (mID == 100)
                {
                    currindex = 0;
                }
                else if (mID == 200)
                {
                    currindex = 1;
                }
                else if (mID == 201)
                {
                    currindex = 2;
                }
                else if (mID == 202)
                {
                    currindex = 3;
                }
                else if (mID == 203)
                {
                    currindex = 4;
                }
                else if (mID == 204)
                {
                    currindex = 5;
                }
                else if (mID == 205)
                {
                    currindex = 6;
                }
                else if (mID == 206)
                {
                    currindex = 7;
                }
                else if (mID == 207)
                {
                    currindex = 8;
                }
                else if (mID == 208)
                {
                    currindex = 9;
                }
                else if (mID == 500)
                {
                    currindex = 10;
                }else
                {
                    currindex = 100;
                }

            if (currindex == 100)
            {
                for (int i = 0; i < 11; i++)
                {
                    if (filled[i] == true)
                    {
                        currindex = i;
                    }
                }
            }
            if (!filled[currindex])
            {
                filled[currindex] = true;
                this.m_set[currindex].setMessageId(mID);
                this.m_set[currindex].setMessageTime(m_time);
                this.m_set[currindex].setf0(m_f0);
                this.m_set[currindex].setf1(m_f1);
                this.m_set[currindex].setf2(m_f2);
                this.m_set[currindex].setf3(m_f3);
                this.m_set[currindex].setf4(m_f4);
                this.m_set[currindex].setf5(m_f5);
                this.m_set[currindex].setf6(m_f6);
                this.m_set[currindex].setf7(m_f7);
            }
                
           

            this.isComplete = chackIfIsComplete();

        }
        public bool chackIfIsComplete()
        {

            return filled[0] && filled[1] && filled[2] && filled[3] && filled[4] && filled[5] && filled[6] && filled[7] && filled[8] && filled[9] && filled[10];

        }

    }
}
