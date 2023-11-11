using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DivEdit
{

    internal class ReadAndWriteFile
    {
        static List<byte> start = new List<byte>();
        static List<byte> end = new List<byte>();
        static int countEgg;
        static ObjectsInfo[] objectsInfo = new ObjectsInfo[11264]; //Массив объектов класса ObjectsInfo с описанием объектов
        public static void writeWorld(String inpFile, int[,,] outArray)// Записываем файл World
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(inpFile, FileMode.Create)))
            {
                int bufStart = 4096;
                writer.Write(4096);
                for (int i = 0; i < 1023; i++)
                {
                    for (int j = 0; j < 512; j++)
                    {
                        bufStart += 18 + outArray[i, j, 5] * 8;
                    }
                    writer.Write(bufStart);
                }
                ushort bufStartSmall = 0;
                for (int i = 0; i < 1024; i++)
                {
                    writer.Write(Convert.ToUInt16(0));
                    for (int j = 0; j < 511; j++)
                    {
                        bufStartSmall += Convert.ToUInt16(16 + outArray[i, j, 5] * 8);
                        writer.Write(bufStartSmall);
                    }
                    bufStartSmall = 0;
                    for (int j = 0; j < 512; j++)
                    {
                        writer.Write(Convert.ToUInt16(outArray[i, j, 0]));
                        writer.Write(Convert.ToUInt16(outArray[i, j, 1]));
                        writer.Write(Convert.ToUInt16(0));
                        writer.Write(Convert.ToByte(outArray[i, j, 5]));
                        writer.Write(Convert.ToUInt16(outArray[i, j, 2]));
                        writer.Write((byte)0);
                        writer.Write(Convert.ToUInt16(outArray[i, j, 3]));
                        writer.Write(Convert.ToUInt16(outArray[i, j, 4]));
                        writer.Write(Convert.ToUInt16(0));
                        for (int k = 0; k < outArray[i, j, 5]; k++)
                        {
                            int XY = outArray[i, j, 6 * k + 7] * 64 + outArray[i, j, 6 * k + 6];
                            int Y = XY / 256;
                            int X = XY % 256;
                            int c = outArray[i, j, 6 * k + 10] / 4096;
                            int b = (outArray[i, j, 6 * k + 10] - c * 4096) / 16;
                            int a = (outArray[i, j, 6 * k + 10] - c * 4096) - b * 16;
                            a = a * 16 + Y;
                            int e = outArray[i, j, 6 * k + 9] / 64;
                            int d = (outArray[i, j, 6 * k + 9] % 64) * 4;
                            writer.Write((byte)X);
                            writer.Write((byte)a);
                            writer.Write((byte)b);
                            writer.Write((byte)c);
                            writer.Write((byte)outArray[i, j, 6 * k + 8]);
                            writer.Write((byte)d);
                            writer.Write((byte)e);
                            writer.Write((byte)outArray[i, j, 6 * k + 11]);
                        }
                    }
                }
                writer.Write(103);
            }
        }
        //====================================================================================================================================================
        public static int[,,] readWorld(String inpFile)// Читаем файл World
        {
            int[,,] tileArray = new int[1024, 512, 96];
            byte[] checksumAll = new byte[4096];
            byte[] checksum = new byte[1024];
            byte[] tile = new byte[9];
            byte[] object_ = new byte[8];
            int XY;
            int buf1;
            if (File.Exists(inpFile))
            {
                using (BinaryReader world = new BinaryReader(File.Open(inpFile, FileMode.Open)))
                {
                    checksumAll = world.ReadBytes(4096);
                    for (int y = 0; y < 1024; y++)
                    {
                        checksum = world.ReadBytes(1024);
                        for (int x = 0; x < 512; x++)
                        {
                            tileArray[y, x, 0] = world.ReadUInt16(); //Полные текстуры
                            tileArray[y, x, 1] = world.ReadUInt16(); //Половинчатые текстуры
                            buf1 = world.ReadUInt16();
                            tileArray[y, x, 5] = world.ReadByte(); //Количество объектов
                            tileArray[y, x, 2] = world.ReadUInt16(); //Эффекты плитки
                            buf1 = world.ReadByte();
                            tileArray[y, x, 3] = world.ReadUInt16(); //var1
                            tileArray[y, x, 4] = world.ReadUInt16(); //var2
                            buf1 = world.ReadUInt16();
                            for (int i = 0; i < tileArray[y, x, 5]; i++)
                            {
                                object_ = world.ReadBytes(8);
                                XY = object_[0] + object_[1] % 16 * 4 * 64;
                                tileArray[y, x, 6 * i + 6] = XY % 64; //Х кор
                                tileArray[y, x, 6 * i + 7] = XY / 64; //Y кор
                                tileArray[y, x, 6 * i + 10] = object_[1] / 16 + object_[2] * 16 + object_[3] * 4096; //Номер
                                tileArray[y, x, 6 * i + 8] = object_[4]; //Высота
                                tileArray[y, x, 6 * i + 9] = object_[5] / 4 + object_[6] * 64; //Имя
                                tileArray[y, x, 6 * i + 11] = object_[7]; //var3
                            }
                        }
                    }
                }
            }
            return tileArray;
        }
        //====================================================================================================================================================
        public static int readObjectsCount(String inpFile)// Читаем количество объектов в файле Objects
        {
            long objCount = 0;
            if (File.Exists(inpFile))
            {
                System.IO.FileInfo file = new System.IO.FileInfo(inpFile);
                objCount = file.Length / 28;
            }
            return (int)objCount;
        }
        //====================================================================================================================================================
        public static int[,] readObjects(String inpFile, int maxObjectCount)// Читаем файл Objects
        {
            int[,] objects = new int[maxObjectCount, 25];
            if (File.Exists(inpFile))
            {
                System.IO.FileInfo file = new System.IO.FileInfo(inpFile);
                long objCount = file.Length / 28;
                using (BinaryReader obj = new BinaryReader(File.Open(inpFile, FileMode.Open)))
                {
                    for (long i = 0; i < objCount; i++)
                    {
                        for (int j = 0; j < 20; j++)
                        {
                            objects[i, j] = obj.ReadByte();
                        }
                        objects[i, 20] = obj.ReadUInt16();
                        objects[i, 21] = obj.ReadUInt16();
                        objects[i, 22] = obj.ReadByte();
                        objects[i, 23] = obj.ReadByte();
                        objects[i, 24] = obj.ReadUInt16();
                    }
                }
            }
            return objects;
        }
        //====================================================================================================================================================
        public static List<int[]> readObjects_2(String inpFile)// Читаем файл Objects
        {
            List<int[]> objct = new List<int[]>();
            if (File.Exists(inpFile))
            {
                System.IO.FileInfo file = new System.IO.FileInfo(inpFile);
                long objCount = file.Length / 28;
                using (BinaryReader obj = new BinaryReader(File.Open(inpFile, FileMode.Open)))
                {
                    for (long i = 0; i < objCount; i++)
                    {
                        objct.Add(new int[25]);
                        for (int j = 0; j < 20; j++)
                        {
                            objct[(int)i][j] = obj.ReadByte();
                        }
                        objct[(int)i][20] = obj.ReadUInt16();
                        objct[(int)i][21] = obj.ReadUInt16();
                        objct[(int)i][22] = obj.ReadByte();
                        objct[(int)i][23] = obj.ReadByte();
                        objct[(int)i][24] = obj.ReadUInt16();
                    }
                }
            }
            return objct;
        }
        //====================================================================================================================================================
        public static void writeObjects(String inpFile, int[,] outArray, int objCount)// Записываем файл World
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(inpFile, FileMode.Create)))
            {

                for (int i = 0; i < objCount; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        writer.Write((byte)outArray[i, j]);
                    }
                    writer.Write(Convert.ToUInt16(outArray[i, 20]));
                    writer.Write(Convert.ToUInt16(outArray[i, 21]));
                    writer.Write((byte)outArray[i, 22]);
                    writer.Write((byte)outArray[i, 23]);
                    writer.Write(Convert.ToUInt16(outArray[i, 24]));
                }
            }
        }
        //====================================================================================================================================================
        public static void writeObjects_2(String inpFile, List<int[]> obj)// Записываем файл World
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(inpFile, FileMode.Create)))
            {

                for (int i = 0; i < obj.Count; i++)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        writer.Write((byte)obj[i][j]);
                    }
                    writer.Write(Convert.ToUInt16(obj[i][20]));
                    writer.Write(Convert.ToUInt16(obj[i][21]));
                    writer.Write((byte)obj[i][22]);
                    writer.Write((byte)obj[i][23]);
                    writer.Write(Convert.ToUInt16(obj[i][24]));
                }
            }
        }
        //====================================================================================================================================================
        public static ObjectsInfo[] readObjectsInfo(string inpFile)// Читаем информацию об объектах
        {
            int a = 0;
            using StreamReader reader = new StreamReader(inpFile);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                objectsInfo[a] = new ObjectsInfo();
                string[] words = line.Split(new char[] { '|' });
                objectsInfo[a].name = words[0];
                Int32.TryParse(words[1], out objectsInfo[a].width);
                Int32.TryParse(words[2], out objectsInfo[a].height);
                Int32.TryParse(words[3], out objectsInfo[a].touchPointX);
                Int32.TryParse(words[4], out objectsInfo[a].touchPointY);
                Int32.TryParse(words[5], out objectsInfo[a].var_1);
                Int32.TryParse(words[6], out objectsInfo[a].var_2);
                Int32.TryParse(words[7], out objectsInfo[a].var_3);
                objectsInfo[a].TP.X = objectsInfo[a].touchPointX;
                objectsInfo[a].TP.Y = objectsInfo[a].touchPointY;
                objectsInfo[a].SP0.X = 0;
                objectsInfo[a].SP0.Y = objectsInfo[a].height;
                objectsInfo[a].SP2.Y = objectsInfo[a].height - (objectsInfo[a].height - objectsInfo[a].touchPointY) * 2;
                objectsInfo[a].SP2.X = objectsInfo[a].touchPointX * 2;
                objectsInfo[a].SP3.Y = objectsInfo[a].height;
                objectsInfo[a].SP3.X = objectsInfo[a].touchPointX * 2 - (objectsInfo[a].SP0.Y - objectsInfo[a].SP2.Y);
                objectsInfo[a].SP1.Y = objectsInfo[a].SP2.Y;
                objectsInfo[a].SP1.X = objectsInfo[a].touchPointX * 2 - objectsInfo[a].SP3.X;
                a++;
            }
            return objectsInfo;
        }
        //====================================================================================================================================================
        public static List<Terrain> ReadTerrain(string inpFile)// Читаем информацию о текстурах
        {
            List<Terrain> ter = new List<Terrain>();
            using StreamReader reader = new StreamReader(inpFile);
            string line;
            bool terrain = true;
            int terCount = -1;
            string[] words;
            while ((line = reader.ReadLine()) != null && terrain)
            {
                words = line.Split(new char[] { ' ' });
                if (words[0] == "endsection" && words[1] == "terrain") terrain = false;
                if (words[0] == "startdef" && words[1] == "terrain")
                {
                    terCount++;
                    ter.Add(new Terrain());
                    ter[terCount].setTerrain(words[2]);

                }
                if (words[0] == "transition") ter[terCount].setTransition(words[1]);
                if (words[0] == "system") ter[terCount].setSystem(int.Parse(words[1]));
                if (words[0] == "tile" && words[1] == "base") ter[terCount].addBaseTile(int.Parse(words[3]));
                if (words[0] == "tile" && words[1] == "transition") ter[terCount].addTrns(int.Parse(words[3]), int.Parse(words[5]));
            }
            return ter;
        }
        //====================================================================================================================================================
        public static List<Metaobject> ReadMetaobject(string inpFile)// Читаем информацию о текстурах
        {
            List<Metaobject> met = new List<Metaobject>();
            using StreamReader reader = new StreamReader(inpFile);
            string line;
            bool metaobject = false;
            int metCount = -1;
            string[] words;
            while ((line = reader.ReadLine()) != null && !metaobject)
            {
                words = line.Split(new char[] { ' ' });
                if (words[0] == "startsection" && words[1] == "metaobjects") metaobject = true;
            }
            while ((line = reader.ReadLine()) != null && metaobject)
            {
                words = line.Split(new char[] { ' ' });
                if (words[0] == "endsection" && words[1] == "metaobjects") metaobject = false;
                if (words[0] == "startdef" && words[1] == "metaobject")
                {
                    metCount++;
                    met.Add(new Metaobject());
                    if (words.Count() > 3) met[metCount].setMet(words[2] + " " + words[3]);
                    else met[metCount].setMet(words[2]);

                }
                if (words[0] == "group") met[metCount].setGroup(words[1]);
                if (words[0] == "type") met[metCount].setType(words[1]);
                if (words[0] == "location") met[metCount].setLocation(words[1]);
                if (words[0] == "walltype") met[metCount].setWalltype(words[1] + " " + words[2]);
                //if (words[0] == "placement") met[metCount].setPlacement(int.Parse(words[2]));
                if (words[0] == "size")
                {
                    met[metCount].setSize(int.Parse(words[1]), int.Parse(words[2]));
                }
                if (words[0] == "object")
                {
                    int[] buff = new int[4];
                    int a = 0;
                    for (int i = 0; i < buff.Length; i++) 
                    { 
                        if (int.TryParse(words[i], out int b))
                        {
                            buff[a] = b;
                            a++;
                        }
                    }
                    met[metCount].addObject(buff);
                }
            }
            return met;
        }
        //====================================================================================================================================================
        public static List<int[]> ReadEggs(string inpFile)// Читаем информацию о точках спавна
        {
            start.Clear();
            end.Clear();
            bool eggsRead = false;
            String buff = "aaaaaa";
            List<int[]> eggs = new List<int[]>();
            if (File.Exists(inpFile))
            {
                byte[] buffer = File.ReadAllBytes(inpFile);
                Console.WriteLine(buffer.Length);
                for (long i = 0; i < buffer.Length; i++)
                {
                    if (!eggsRead) start.Add(buffer[i]);
                    buff = buff.Substring(1) + Convert.ToChar(buffer[i]);
                    if (buff == "EggsV0")
                    {
                        i += 17;
                        countEgg = buffer[i + 1] * 256 + buffer[i];
                        Console.WriteLine(countEgg);
                        i += 4;
                        for (int j = 0; j < countEgg; j++)
                        {
                            int a = 0;
                            eggs.Add(new int[15]);
                            for (int k = 0; k < 46; k++)
                            {
                                if (k < 19 && k != 1 && k != 3 && k != 5 && k != 7)
                                {
                                    eggs[j][a] = buffer[i + 1] * 256 + buffer[i];
                                    a++;
                                }
                                i++;
                                i++;
                            }
                        }
                        eggsRead = true;
                    }
                    if (eggsRead) end.Add(buffer[i]);
                }
            }
            start.RemoveRange(start.Count - 6, 6);
            return eggs;
        }
        public static void WriteEggs(string inpFile, List<int[]> eggs)// Записываем информацию о точках спавна
        {
            countEgg = eggs.Count;
            using (BinaryWriter writer = new BinaryWriter(File.Open(inpFile, FileMode.Create)))
            {
                for (int i = 0; i < start.Count; i++)
                {
                    writer.Write(start[i]);
                }
                byte[] rawData = { 0x45, 0x67, 0x67, 0x73, 0x56, 0x30, 0x2E, 0x39, 0x33, 0x35, 0x20, 0x32, 0x35, 0x2D, 0x30, 0x32, 0x2D, 0x32, 0x30, 0x30, 0x32 };
                writer.Write(rawData);
                writer.Write((byte)0);
                writer.Write(countEgg);
                for (int i = 0; i < countEgg; i++)
                {
                    for (int j = 0; j < 15; j++)
                    {
                        if (j != 12) writer.Write((ushort)eggs[i][j]);
                        else writer.Write((ushort)i);
                        if (j < 4) writer.Write((ushort)0);

                    }
                    for (int j = 0; j < 54; j++) writer.Write((byte)0);
                }
                for (int i = 0; i < end.Count; i++)
                {
                    writer.Write(end[i]);
                }
            }
        }
    }
}
