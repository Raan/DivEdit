/*   objectsInFrame
0_Имя
1_плитка по X
2_плитка по Y
3_номер на плитке
4_положение по X
5_положение по Y
6_высота
7_мировая координата
8_номер
    tiles
0_fullText
1_halfText
2_tileEffect
3_var1
4_var2 (var2 == var1)
5_objCount
6_xCor
7_yCor
8_height
9_objName
10_objNum
11_var3
    objects
0_var 0
...
19_var 19
20_координата х
21_координата у
22_var 20
23_var 21
24_имя
*/
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Point = Microsoft.Xna.Framework.Point;
using Graphics = Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using SharpDX.Direct3D9;

namespace DivEdit
{
    class Terrain
    {
        public String terrain;
        public String transition;
        public int system;
        public List<int> baseTile = new List<int>();
        public List<int>[] trns = new List<int>[16];
        static int count = 0;
        public Terrain()
        {
            count++;
            for (int i = 0; i < 16; i++) trns[i] = new List<int>();
        }
        public void setTerrain(String ter)
        {
            this.terrain = ter;
        }
        public void setTransition(String trn)
        {
            this.transition = trn;
        }
        public void setSystem(int sys)
        {
            this.system = sys;
        }
        public void addBaseTile(int tile)
        {
            baseTile.Add(tile);
        }
        public int getBaseTile()
        {
            Random rnd = new Random();
            return baseTile[rnd.Next(0, baseTile.Count - 1)];
        }
        public void addTrns(int num, int trn)
        {
            trns[num].Add(trn);
        }
        public int getTrns(int num)
        {
            Random rnd = new Random();
            if (trns[num].Count > 0) return trns[num][rnd.Next(0, trns[num].Count - 1)];
            else return 0;
        }
        public static int TotalCount()
        {
            return count;
        }
    }
    public class ObjectsInfo
    {
        //     
        //   A1|--------------------------|A2
        //     |                          |
        //     |                          |
        //     |                          |
        //     |                          |
        //     |                          |
        //     |                          |
        //     |                          |
        //     |              SP1 /-------|SP2
        //     |                /       / |
        //     |              /       /   |
        //     |            /       /     |
        //     |          /  TP   /       |
        //     |        /   .   /         |
        //     |      /       /           |
        //     |    /       /             |
        //     |  /       /               |
        //     |/SP0    /SP3              |
        //   A0|--------------------------|A3
        //
        public string name;             // Имя объекта
        public int height;              // Высота спрайта объета
        public int width;               // Ширина спрайта
        public int touchPointX;         // Точка касания по х
        public int touchPointY;         // Точка касания по y
        public int var_1;               // 
        public int var_2;               // 
        public int var_3;               // 
        public Point SP0 = new Point();
        public Point SP1 = new Point();
        public Point SP2 = new Point();
        public Point SP3 = new Point();
        public Point TP = new Point();
    }
    internal class GameData
    {
        private String WorldFile;
        private String ObjFile;
        private int[,,] tiles = new int[1024, 512, 96];                     // Основной массив данных world.00x
        private List<int[]> objects = new();                                // [25] Основной массив объектов 
        private List<int[]> objectsInFrame = new();                         // [9] Имя, плитка по X, плитка по Y, номер на плитке, положение по X, положение по Y, высота, мировая координата, номер 
        private List<int> obgSort = new();                                  // Сортировка объектов 
        private static ObjectsInfo[] objectsInfo = new ObjectsInfo[11264];  // Массив объектов класса ObjectsInfo с описанием объектов
        private int[] buffObjectCopy = new int[35];                         //Буфферный массив для копирования или перемещения объекта
        bool objectCopy;
        private int cursorOffsetX;
        private int cursorOffsetY;
        List<Terrain> terrain = new();



        public GameData() { }
        public void Initialize(String WFile, String OFile)
        {
            WorldFile = WFile;
            ObjFile = OFile;
            tiles = ReadAndWriteFile.readWorld(WorldFile);
            objects = ReadAndWriteFile.readObjects_2(ObjFile);
            objectsInfo = ReadAndWriteFile.readObjectsInfo(@"objects.de");
            terrain = ReadAndWriteFile.ReadTerrain(@"editor.dat");
        }
        public String GetFullTileTexturesName(int x, int y)
        {
            if (tiles[y, x, 0] < 9369)
            {
                return "floor/" + tiles[y, x, 0].ToString().PadLeft(6, '0');
            }
            else return "floor/003271";
        }
        public String GetHalfTileTexturesName(int x, int y)
        {
            if (tiles[y, x, 1] < 9369)
            {
                return "floor/" + tiles[y, x, 1].ToString().PadLeft(6, '0');
            }
            else return "floor/003271";
        }
        public int[,,] GetTiles()
        {
            return tiles;
        }
        public List<int[]> GetObjects()
        {
            return objects;
        }
        public void SetObject(int n, int[] obj)
        {
            objects[n] = obj;
        }
        public int[] GetObject(int num)
        {
            if (num < objects.Count)
            {
                return objects[num];

            }
            else return objects[0];
        }
        public int[] GetTile(int x, int y)
        {
            int[] tile = new int[5];
            tile[0] = tiles[y, x, 0];
            tile[1] = tiles[y, x, 1];
            tile[2] = tiles[y, x, 2];
            tile[3] = tiles[y, x, 3];
            tile[4] = tiles[y, x, 4];
            return tile;
        }
        public void SetTile(int[] tile, int x, int y)
        {
            tiles[y, x, 0] = tile[0];
            tiles[y, x, 1] = tile[1];
            tiles[y, x, 2] = tile[2];
            tiles[y, x, 3] = tile[3];
            tiles[y, x, 4] = tile[4];
        }
        public int GetTileEffect(int x, int y)
        {
            return tiles[y, x, 2];
        }
        public void UpdateDisplayedObjects(int x, int y, int width, int height, bool moving)
        {
            objectsInFrame.Clear();
            obgSort.Clear();
            for (int i = -5; i < width / 64; i++)
            {
                for (int j = -5; j < height / 64 + 1; j++)
                {
                    if (j + y >= 0 && i + x >= 0)
                    {
                        for (int z = 0; z < tiles[j + y, i + x, 5]; z++)
                        {
                            objectsInFrame.Add(new int[9]);
                            int objCount = objectsInFrame.Count - 1;
                            obgSort.Add(objCount);
                            objectsInFrame[objCount][0] = tiles[j + y, i + x, 9 + z * 6];                                   // Имя
                            objectsInFrame[objCount][1] = i + x;                                                            // плитка по X
                            objectsInFrame[objCount][2] = j + y;                                                            // плитка по Y
                            objectsInFrame[objCount][3] = z;                                                                // номер на плитке
                            objectsInFrame[objCount][4] = i * 64 + tiles[j + y, i + x, 6 + z * 6];                          // положение по X
                            objectsInFrame[objCount][5] = j * 64 + tiles[j + y, i + x, 7 + z * 6];                          // положение по Y
                            objectsInFrame[objCount][6] = tiles[j + y, i + x, 8 + z * 6];                                   // высота
                            int Y1 = objectsInFrame[objCount][5] + objectsInfo[objectsInFrame[objCount][0]].touchPointY;
                            int X1 = objectsInFrame[objCount][4] + objectsInfo[objectsInFrame[objCount][0]].touchPointX;
                            objectsInFrame[objCount][7] = Y1 * width + X1;                                                  // мировая координата
                            objectsInFrame[objCount][8] = tiles[j + y, i + x, 10 + z * 6];                                  // номер
                        }
                    }
                }
            }
            if (moving)
            {
                objectsInFrame.Add(new int[9]);
                int objCount = objectsInFrame.Count - 1;
                obgSort.Add(objCount);
                objectsInFrame[objCount][0] = buffObjectCopy[3];
                objectsInFrame[objCount][1] = 0;
                objectsInFrame[objCount][2] = 0;
                objectsInFrame[objCount][3] = 0;
                objectsInFrame[objCount][4] = buffObjectCopy[6];
                objectsInFrame[objCount][5] = buffObjectCopy[7];
                objectsInFrame[objCount][6] = buffObjectCopy[2];
                int Y1 = objectsInFrame[objCount][5] + objectsInfo[objectsInFrame[objCount][0]].touchPointY;
                int X1 = objectsInFrame[objCount][4] + objectsInfo[objectsInFrame[objCount][0]].touchPointX;
                objectsInFrame[objCount][7] = Y1 * width + X1;
                objectsInFrame[objCount][8] = 0;
            }
            objectsInFrame.Sort((b1, b2) => Sort(b1[7], b2[7]));
        }
        private static int Sort(int a, int b)
        {
            if (a < b) return -1;
            else if (a > b) return 1;
            else if (a == 0) return 0;
            else return 0;
        }
        public int GetObjectCount()
        {
            return objectsInFrame.Count;
        }
        public int[] GetObjectInFrame(int num)
        {
            if (num < objectsInFrame.Count)
            {
                return objectsInFrame[num];

            }
            else return objectsInFrame[0];
        }
        public String GetObjectPath(int n)
        {
            if (n < objectsInFrame.Count)
            {
                return "objects/" + objectsInFrame[n][0].ToString().PadLeft(6, '0');

            }
            else return "objects/001097";
        }
        public int GetObjectName(int n)
        {
            if (n < objectsInFrame.Count)
            {
                return objectsInFrame[n][0];

            }
            else return 0;
        }
        public Vector2 GetObjectPosition(int n)
        {
            if (n < objectsInFrame.Count)
            {
                return new Vector2(objectsInFrame[n][4], objectsInFrame[n][5] - objectsInFrame[n][6]);
            }
            else return new Vector2(0, 0);
        }
        public String[] GetTexturePalette(int textures)
        {
            String[] tex = new string[4];
            if (terrain[textures].baseTile[0] > 0 && terrain[textures].baseTile[0] <= 9368)
            {
                tex[0] = "floor/" + terrain[textures].baseTile[0].ToString().PadLeft(6, '0');
                tex[1] = "floor/" + terrain[textures].baseTile[0].ToString().PadLeft(6, '0');
                tex[2] = "floor/" + terrain[textures].baseTile[0].ToString().PadLeft(6, '0');
                tex[3] = "floor/" + terrain[textures].baseTile[0].ToString().PadLeft(6, '0');
            }
            for (int i = 0; i < terrain[textures].baseTile.Count && i < 4; i++)
            {
                tex[i] = "floor/" + terrain[textures].baseTile[i].ToString().PadLeft(6, '0');
            }
            return tex;
        }
        public void TextureMapping(int textures, int MouseStateX, int MouseStateY, int xCor, int yCor, bool KeyLeftShift, bool KeyLeftControl)
        {
            int y = MouseStateY / 64 + yCor;
            int x = MouseStateX / 64 + xCor;
            int quarter = 0;
            if (MouseStateY % 64 < 32 && MouseStateX % 64 < 32) quarter = 0;
            if (MouseStateY % 64 < 32 && MouseStateX % 64 > 32) quarter = 1;
            if (MouseStateY % 64 > 32 && MouseStateX % 64 > 32) quarter = 2;
            if (MouseStateY % 64 > 32 && MouseStateX % 64 < 32) quarter = 3;
            int[,] tilesNew = new int[3, 3];
            int[,] tileFilling = new int[3, 3];
            if (terrain[textures].transition != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        tilesNew[i, j] = tiles[y + i - 1, x + j - 1, 0];
                        for (int k = 0; k < terrain[textures].baseTile.Count; k++)
                        {
                            if (tilesNew[i, j] == terrain[textures].baseTile[k]) tileFilling[i, j] = 1111;
                        }
                        for (int k = 0; k < 16; k++)
                        {
                            for (int t = 0; t < terrain[textures].trns[k].Count; t++)
                            {
                                if (tilesNew[i, j] == terrain[textures].trns[k][t])
                                {
                                    tileFilling[i, j] = k switch
                                    {
                                        1 => 1100,
                                        2 => 1000,
                                        3 => 1001,
                                        4 => 1,
                                        5 => 11,
                                        6 => 10,
                                        7 => 110,
                                        8 => 100,
                                        9 => 1101,
                                        10 => 1011,
                                        11 => 111,
                                        12 => 1110,
                                        14 => 101,
                                        15 => 1010,
                                        _ => 0,
                                    };
                                }
                            }
                        }
                    }
                }
                // |_0_|_1_|
                // |_3_|_2_|
                if (quarter == 0) tileFilling[1, 1] = (tileFilling[1, 1] / 10) * 10 + 1;
                if (quarter == 1) tileFilling[1, 1] = (tileFilling[1, 1] / 100) * 100 + tileFilling[1, 1] % 10 + 10;
                if (quarter == 2) tileFilling[1, 1] = (tileFilling[1, 1] / 1000) * 1000 + tileFilling[1, 1] % 100 + 100;
                if (quarter == 3) tileFilling[1, 1] = tileFilling[1, 1] % 1000 + 1000;

                if (tileFilling[1, 1] % 10 == 1) //0
                {
                    tileFilling[1, 0] = (tileFilling[1, 0] / 100) * 100 + tileFilling[1, 0] % 10 + 10;
                    tileFilling[0, 0] = (tileFilling[0, 0] / 1000) * 1000 + tileFilling[0, 0] % 100 + 100;
                    tileFilling[0, 1] = tileFilling[0, 1] % 1000 + 1000;
                }
                if ((tileFilling[1, 1] / 10) % 10 == 1) //1
                {
                    tileFilling[0, 1] = (tileFilling[0, 1] / 1000) * 1000 + tileFilling[0, 1] % 100 + 100;
                    tileFilling[0, 2] = tileFilling[0, 2] % 1000 + 1000;
                    tileFilling[1, 2] = (tileFilling[1, 2] / 10) * 10 + 1;
                }
                if ((tileFilling[1, 1] / 100) % 10 == 1) //2
                {
                    tileFilling[1, 2] = tileFilling[1, 2] % 1000 + 1000;
                    tileFilling[2, 2] = (tileFilling[2, 2] / 10) * 10 + 1;
                    tileFilling[2, 1] = (tileFilling[2, 1] / 100) * 100 + tileFilling[2, 1] % 10 + 10;
                }
                if ((tileFilling[1, 1] / 1000) % 10 == 1) //3
                {
                    tileFilling[2, 1] = (tileFilling[2, 1] / 10) * 10 + 1;
                    tileFilling[2, 0] = (tileFilling[2, 0] / 100) * 100 + tileFilling[2, 0] % 10 + 10;
                    tileFilling[1, 0] = (tileFilling[1, 0] / 1000) * 1000 + tileFilling[1, 0] % 100 + 100;
                }
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (tileFilling[i, j] == 1111)
                        {
                            bool a = false;
                            for (int k = 0; k < terrain[textures].baseTile.Count; k++)
                            {
                                if (tilesNew[i, j] == terrain[textures].baseTile[k]) a = true;
                            }
                            if (!a) tilesNew[i, j] = terrain[textures].getBaseTile();
                        }
                        else
                        {
                            switch (tileFilling[i, j])
                            {
                                case 1100: { int a = terrain[textures].getTrns(1); if (a != 0) tilesNew[i, j] = a; break; }
                                case 1000: { int a = terrain[textures].getTrns(2); if (a != 0) tilesNew[i, j] = a; break; }
                                case 1001: { int a = terrain[textures].getTrns(3); if (a != 0) tilesNew[i, j] = a; break; }
                                case 1: { int a = terrain[textures].getTrns(4); if (a != 0) tilesNew[i, j] = a; break; }
                                case 11: { int a = terrain[textures].getTrns(5); if (a != 0) tilesNew[i, j] = a; break; }
                                case 10: { int a = terrain[textures].getTrns(6); if (a != 0) tilesNew[i, j] = a; break; }
                                case 110: { int a = terrain[textures].getTrns(7); if (a != 0) tilesNew[i, j] = a; break; }
                                case 100: { int a = terrain[textures].getTrns(8); if (a != 0) tilesNew[i, j] = a; break; }
                                case 1101: { int a = terrain[textures].getTrns(9); if (a != 0) tilesNew[i, j] = a; break; }
                                case 1011: { int a = terrain[textures].getTrns(10); if (a != 0) tilesNew[i, j] = a; break; }
                                case 111: { int a = terrain[textures].getTrns(11); if (a != 0) tilesNew[i, j] = a; break; }
                                case 1110: { int a = terrain[textures].getTrns(12); if (a != 0) tilesNew[i, j] = a; break; }
                                case 101: { int a = terrain[textures].getTrns(14); if (a != 0) tilesNew[i, j] = a; break; }
                                case 1010: { int a = terrain[textures].getTrns(15); if (a != 0) tilesNew[i, j] = a; break; }
                            }
                        }
                        tiles[y + i - 1, x + j - 1, 0] = tilesNew[i, j];
                    }
                }
            }
            else
            {
                if (!KeyLeftShift)
                {
                    if (!KeyLeftControl)
                    {
                        bool a = false;
                        for (int k = 0; k < terrain[textures].baseTile.Count; k++)
                        {
                            if (tiles[y, x, 0] == terrain[textures].baseTile[k]) a = true;
                        }
                        if (!a) tiles[y, x, 0] = terrain[textures].getBaseTile();
                    }
                    else
                    {
                        tiles[y, x, 0] = terrain[textures].getBaseTile();
                    }
                }
                else
                if (!KeyLeftControl)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            bool a = false;
                            for (int k = 0; k < terrain[textures].baseTile.Count; k++)
                            {
                                if (tiles[y + i - 1, x + j - 1, 0] == terrain[textures].baseTile[k]) a = true;
                            }
                            if (!a) tiles[y + i - 1, x + j - 1, 0] = terrain[textures].getBaseTile();
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            tiles[y + i - 1, x + j - 1, 0] = terrain[textures].getBaseTile();
                        }
                    }
                }
            }
        }
        public int ObjectDel(int selectedObject)
        {
            int x = objectsInFrame[selectedObject][1];
            int y = objectsInFrame[selectedObject][2];
            int z = objectsInFrame[selectedObject][3];
            int num = objectsInFrame[selectedObject][8];
            for (int i = z; i < tiles[y, x, 5]; i++)
            {
                tiles[y, x, 6 + i * 6] = tiles[y, x, 6 + (i + 1) * 6];
                tiles[y, x, 7 + i * 6] = tiles[y, x, 7 + (i + 1) * 6];
                tiles[y, x, 8 + i * 6] = tiles[y, x, 8 + (i + 1) * 6];
                tiles[y, x, 9 + i * 6] = tiles[y, x, 9 + (i + 1) * 6];
                tiles[y, x, 10 + i * 6] = tiles[y, x, 10 + (i + 1) * 6];
                tiles[y, x, 11 + i * 6] = tiles[y, x, 11 + (i + 1) * 6];
            }
            tiles[y, x, 5]--;
            for (int i = 0; i < 25; i++)
            {
                objects[num][i] = 255;
            }
            objects[num][20] = 65535;
            objects[num][21] = 65535;
            objects[num][24] = 65535;
            return -1;
        }
        public bool StartMovingAnObject(int selectedObject, int mousX, int mousY, int xCor, int yCor, bool Ctrl)
        {

            //Если курсор в области выделенного объекта
            if (mousX > objectsInFrame[selectedObject][4] &&
                mousX < objectsInFrame[selectedObject][4] + objectsInfo[objectsInFrame[selectedObject][0]].width &&
                mousY > objectsInFrame[selectedObject][5] - objectsInFrame[selectedObject][6] &&
                mousY < objectsInFrame[selectedObject][5] + objectsInfo[objectsInFrame[selectedObject][0]].height - objectsInFrame[selectedObject][6])
            {
                cursorOffsetX = mousX - objectsInFrame[selectedObject][4];
                cursorOffsetY = mousY - objectsInFrame[selectedObject][5];
                int x = objectsInFrame[selectedObject][1];
                int y = objectsInFrame[selectedObject][2];
                int z = objectsInFrame[selectedObject][3];
                buffObjectCopy[0] = tiles[y, x, 6 + z * 6];
                buffObjectCopy[1] = tiles[y, x, 7 + z * 6];
                buffObjectCopy[2] = tiles[y, x, 8 + z * 6];
                buffObjectCopy[3] = tiles[y, x, 9 + z * 6];
                buffObjectCopy[4] = tiles[y, x, 10 + z * 6];
                buffObjectCopy[5] = tiles[y, x, 11 + z * 6];
                buffObjectCopy[6] = xCor * 64 + mousX - cursorOffsetX;
                buffObjectCopy[7] = yCor * 64 + mousY - cursorOffsetY;
                buffObjectCopy[8] = selectedObject;
                for (int i = 0; i < 25; i++)
                {
                    buffObjectCopy[9 + i] = objects[buffObjectCopy[4]][i];
                }
                if (Ctrl)
                {
                    buffObjectCopy[4] = objects.Count;
                    objectCopy = true;
                }
                else
                {
                    for (int i = z; i < tiles[y, x, 5]; i++)
                    {
                        tiles[y, x, 6 + i * 6] = tiles[y, x, 6 + (i + 1) * 6];
                        tiles[y, x, 7 + i * 6] = tiles[y, x, 7 + (i + 1) * 6];
                        tiles[y, x, 8 + i * 6] = tiles[y, x, 8 + (i + 1) * 6];
                        tiles[y, x, 9 + i * 6] = tiles[y, x, 9 + (i + 1) * 6];
                        tiles[y, x, 10 + i * 6] = tiles[y, x, 10 + (i + 1) * 6];
                        tiles[y, x, 11 + i * 6] = tiles[y, x, 11 + (i + 1) * 6];
                    }
                    tiles[y, x, 5]--;
                    objectCopy = false;
                }
                return true;
            }
            else
            {
                cursorOffsetX = 0;
                cursorOffsetY = 0;
                return false;
            }
        }
        public void MovingAnObject(bool Shift, int mouseX, int mouseY)
        {
            if (Shift)
            {
                buffObjectCopy[6] = ((mouseX - cursorOffsetX) / 64) * 64 + buffObjectCopy[0];
                buffObjectCopy[7] = ((mouseY - cursorOffsetY) / 64) * 64 + buffObjectCopy[1];
            }
            else
            {
                buffObjectCopy[6] = mouseX - cursorOffsetX;
                buffObjectCopy[7] = mouseY - cursorOffsetY;
            }
        }
        public bool PasteObjectAfterMove(int xCor, int yCor, int mouseX, int mouseY, bool Shift)
        {
            int insertionCorX = xCor + (mouseX - cursorOffsetX) / 64;
            int insertionCorY = yCor + (mouseY - cursorOffsetY) / 64;
            int objCountInTile = tiles[insertionCorY, insertionCorX, 5];
            if (objCountInTile < 15)
            {
                if (Shift)
                {
                    tiles[insertionCorY, insertionCorX, 6 + objCountInTile * 6] = buffObjectCopy[0];
                    tiles[insertionCorY, insertionCorX, 7 + objCountInTile * 6] = buffObjectCopy[1];
                }
                else
                {
                    tiles[insertionCorY, insertionCorX, 6 + objCountInTile * 6] = (mouseX - cursorOffsetX) % 64;
                    tiles[insertionCorY, insertionCorX, 7 + objCountInTile * 6] = (mouseY - cursorOffsetY) % 64;
                }
                tiles[insertionCorY, insertionCorX, 8 + objCountInTile * 6] = buffObjectCopy[2];
                tiles[insertionCorY, insertionCorX, 9 + objCountInTile * 6] = buffObjectCopy[3];
                tiles[insertionCorY, insertionCorX, 10 + objCountInTile * 6] = buffObjectCopy[4];
                tiles[insertionCorY, insertionCorX, 11 + objCountInTile * 6] = buffObjectCopy[5];
                tiles[insertionCorY, insertionCorX, 5]++;
                if (objectCopy)
                {
                    objects.Add(new int[25]);
                    for (int k = 0; k < 25; k++)
                    {
                        objects[^1][k] = buffObjectCopy[9 + k];
                    }
                    objects[^1][20] = insertionCorX * 64 + tiles[insertionCorY, insertionCorX, 6 + objCountInTile * 6];
                    objects[^1][21] = insertionCorY * 64 + tiles[insertionCorY, insertionCorX, 7 + objCountInTile * 6];
                }
                else
                {
                    objects[buffObjectCopy[4]][20] = insertionCorX * 64 + tiles[insertionCorY, insertionCorX, 6 + objCountInTile * 6];
                    objects[buffObjectCopy[4]][21] = insertionCorY * 64 + tiles[insertionCorY, insertionCorX, 7 + objCountInTile * 6];
                }
                objectCopy = false;
            }
            return false;
        }
        public void ChangingHeightObject (int selObj, bool direction)
        {
            if (direction && tiles[objectsInFrame[selObj][2], objectsInFrame[selObj][1], 8 + objectsInFrame[selObj][3] * 6] < 255) tiles[objectsInFrame[selObj][2], objectsInFrame[selObj][1], 8 + objectsInFrame[selObj][3] * 6]++;
            if (!direction && tiles[objectsInFrame[selObj][2], objectsInFrame[selObj][1], 8 + objectsInFrame[selObj][3] * 6] > 0) tiles[objectsInFrame[selObj][2], objectsInFrame[selObj][1], 8 + objectsInFrame[selObj][3] * 6]--;
        }
    }
}
