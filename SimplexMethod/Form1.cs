using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SimplexMethod
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "3";
            textBox2.Text = "3";

            int col = Int32.Parse(textBox1.Text.ToString()) + 1;
            int row = Int32.Parse(textBox2.Text.ToString()) + 1;

            dataGridView1.ColumnCount = col;
            dataGridView1.RowCount = row;
            for (int i = 0; i < (dataGridView1.ColumnCount-1); i++)
            {
                dataGridView1.Columns[i].HeaderText = "x" + (i+1);
            }

            //TestFill();

            //double[,] test_mtrx = new double[,] { { 2, 1, 1, 0, 0, 0, 800 }, { 0, 1, 0, 2, 1, 0, 900 }, { 0, 0, 1, 1, 2, 3, 6000 }, { -0.4, -1.1, -1.4, 0, -0.3, -0.6, 0 } };
            double[,] test_mtrx = new double[,] { { 2, 1, 3, 1 }, { -1,3,-1,3 }, { 1,11,3,11 },{-1,1,0,1} };
            double[,] s_table = new double[row+1,col+row];

            //Формирование матрицы с искуственным базисом без f(x) и базисных значений
            for (int i = 0; i < s_table.GetLength(0)-1; i++) {
                for (int j = 0;j < s_table.GetLength(1)-1; j++) {
                    if (isset(test_mtrx, i, j)&&(i<test_mtrx.GetLength(0)-1)&&(j<test_mtrx.GetLength(1)-1))
                    {
                        s_table[i, j] = test_mtrx[i, j];
                    }
                    else {
                        if ((j-(test_mtrx.GetLength(1)-1)==i)&&(i<test_mtrx.GetLength(0)-1))
                        {
                            s_table[i, j] = 1;
                        }
                        else
                        {
                            s_table[i, j] = 0;
                        }
                    }
                }
            }
            //Добавление базисных значений (количество заготовок)
            for (int i = 0; i < test_mtrx.GetLength(0); i++)
            {
                s_table[i, s_table.GetLength(1) - 2] = test_mtrx[i, test_mtrx.GetLength(1) - 1];
            }

            //Добавление f(x) (остатков раскроя)
            for (int i = 0; i < test_mtrx.GetLength(1); i++) {
                s_table[s_table.GetLength(0) - 2, i] = test_mtrx[test_mtrx.GetLength(0) - 1, i];
            }

            //Добавление z(x) (функция искуственных базисов)
            s_table = find_z_row(s_table, row-1,col-1);

            //Поиск результирующего элемента
            int[] result_indexes = new int[2];
            do
            {
                result_indexes = find_result_element(s_table);
                s_table = recount_result_lines(s_table, result_indexes);
                double s = s_table[s_table.GetLength(0) - 1, s_table.GetLength(1) - 2];
            }
            while (s_table[s_table.GetLength(0) - 1, s_table.GetLength(1) - 2]>0);

            print_array(s_table);

            if (result_indexes[0] > -1 && result_indexes[1] > -1)
            {
                dataGridView1.Rows[result_indexes[1]].Cells[result_indexes[0]].Style.BackColor = Color.Green;
            }
        }

        private void TestFill() {
            double[,] mtrx = new double[,] { {2, 1, 1, 0, 0, 0,800 },{0, 1, 0, 2, 1, 0,900 },{ 0, 0, 1, 1, 2, 3,6000},{ -0.4,-1.1,-1.4,0,-0.3,-0.6,0} };
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 7; j++) {
                    dataGridView1.Rows[i].Cells[j].Value = mtrx[i, j];
                }
            }
        }

        private bool isset(double[,] x,int i,int j) {
            try {
                double z = x[i,j];
                return true;
            }
            catch (Exception ex) {
                return false;
            }
        }

        //Добавление z(x) (функция искуственных базисов)
        private double[,] find_z_row(double[,] arr,int basis_row_count,int var_count) {
            for (int i = 0; i < arr.GetLength(1); i++) {
                if (i < var_count || i > var_count + basis_row_count-1)
                {
                    for (int j = 0; j < arr.GetLength(0) - 2; j++)
                    {
                        arr[arr.GetLength(0) - 1, i] = arr[arr.GetLength(0) - 1, i] + arr[j, i];
                    }
                }
            }
            return arr;
        }

        //Поиск результирующего элемента
        private int[] find_result_element(double[,] arr) {

            double max = 0;
            int index_max = -1;
            int index_min = -1;
            int[] index_res = new int[2];
            for (int j = 0; j < arr.GetLength(1) - 2; j++) {
                if (arr[arr.GetLength(0) - 1, j] > max) {
                    max = arr[arr.GetLength(0) - 1, j];
                    index_max = j;
                }
            }

            double min_res = 0;
            for (int i = 0; i < arr.GetLength(0)-2; i++) {
                if (arr[i, index_max] != 0)
                {
                    double temp = arr[i, arr.GetLength(1) - 2] / arr[i, index_max];
                    if (min_res == 0 || min_res > temp)
                    {
                        min_res = temp;
                        index_min = i;
                    }
                }
            }
            index_res[0] = index_max;
            index_res[1] = index_min;
            return index_res;
        }

        //Пересчет строки и столбца результирующего массива
        private double[,] recount_result_lines(double[,] arr,int[] indexes) {
            double result_element = arr[indexes[1], indexes[0]];
            double[,] old_arr = new double[arr.GetLength(0),arr.GetLength(1)];
            old_arr = arr;

            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (i != indexes[1] && j != indexes[0])
                    {
                        Debug.WriteLine(old_arr[i, j] + "-(" + old_arr[i, indexes[0]] + "*" + old_arr[indexes[1], j] + ")/" + result_element);
                        double k = old_arr[i, j] - (old_arr[i, indexes[0]] * old_arr[indexes[1], j]) / result_element;
                        arr[i, j] = k;

                    }
                    //Debugger.Break();
                }
            }

            for (int i=0;i<arr.GetLength(1); i++) {
                arr[indexes[1],i] = Math.Round(arr[indexes[1],i] / result_element,2);
            }
            for (int j = 0; j < arr.GetLength(0); j++) {
                if(j!=indexes[1])
                    arr[j, indexes[0]] = 0;
            }
            
            return arr;
        }

        //Вывод массива на экран
        private void print_array(double[,] arr) {
            dataGridView1.Rows.Clear();
            dataGridView1.RowCount = arr.GetLength(0);
            dataGridView1.ColumnCount = arr.GetLength(1);
            
            for (int i = 0; i < arr.GetLength(0); i++) {
                for (int j = 0; j < arr.GetLength(1); j++) {
                    if(j<(arr.GetLength(1)-2))
                    dataGridView1.Columns[j].HeaderText = "x" + (j + 1);
                    dataGridView1.Rows[i].Cells[j].Value = arr[i, j];
                }
            }
        }

        private void print_array(List<double[]> arr, List<double> osta, int[] kol)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.RowCount = arr[0].Length+1;
            dataGridView1.ColumnCount = arr.Count()+1;
            int num = 0;
            int kl = 0;
            for (int i = 0; i < arr.Count; i++) {
                kl = i;
                for (int j = 0; j < arr[i].Length; j++) {
                    dataGridView1.Rows[j].Cells[i].Value = arr[i][j];
                    num = j;
                }
            }
            for (int i = 0; i < arr.Count; i++)
                {
                    dataGridView1.Rows[num+1].Cells[i].Value = Math.Round(osta[i], 2);
                }
            int row_dla_null = 0;
            for (int j = 0; j < arr[0].Length; j++)
            {
                dataGridView1.Rows[j].Cells[kl+1].Value = kol[j];
                row_dla_null = j;
            }
            dataGridView1.Rows[row_dla_null+1].Cells[kl + 1].Value = 0;

            int lin = dataGridView1.RowCount;
            int stb = dataGridView1.ColumnCount;
            double[,] matrix = new double[lin, stb];
            for (int j = 0; j < lin; j++)
            {
                for (int i = 0; i < stb; i++)
                {
                    matrix[j, i] = Convert.ToDouble(dataGridView1.Rows[j].Cells[i].Value);   // заполняем матрицу                 
                }
            }
            //Debugger.Break();

        }

        //Функция заполнения вариантов раскроя
        private void button2_Click(object sender, EventArgs e)
        {   
            int gr2kol = dataGridView2.ColumnCount-1;
            int gr2row = dataGridView2.RowCount-1;
            double[] start_type_size = new double[gr2row];
            for (int p = 0; p < gr2row; p++)
            {
                start_type_size[p] = Convert.ToDouble(dataGridView2.Rows[p].Cells[0].Value);
            }
            int[] kolich_d = new int[gr2row];
            for (int p = 0; p < gr2row; p++)
            {
                kolich_d[p] = Convert.ToInt32(dataGridView2.Rows[p].Cells[1].Value);
            }
            //Debugger.Break();
            int start_size = Convert.ToInt32(textBox3.Text);
            //double[] start_type_size = { 6.5, 3.7, 3 };
            //int[] kolich_d = { 600, 800, 6000, 0 };
            double[] type_size = new double[start_type_size.Length];
            double[] result_arr = new double[type_size.Length];
            double[] temp_arr = new double[type_size.Length];
            List<double[]> result_list = new List<double[]>();
            List<double> ostatok = new List<double>();
            bool flag = false;



            do //temp
            {
                int i = 0;
                int last_el = get_last_el(result_arr);
                double temp_size = 0;
                start_type_size.CopyTo(type_size, 0);

                if (last_el < 0)
                {
                    temp_size = start_size;
                }
                else {
                    if (last_el > 0)
                    {
                        double[] tr = new double[result_arr.Length];

                        for (int r = 0; r <= last_el; r++)
                        {
                            tr[r] = (int)result_arr[r];
                        }
                        result_arr = tr;

                        result_arr[last_el] = result_arr[last_el] - 1;
                        temp_size = start_size;

                        for (int r = 0; r <= last_el; r++) {
                            temp_size = temp_size - (double)(result_arr[r] * type_size[r]);
                        }

                    }
                    else
                    {
                        result_arr[last_el] = result_arr[last_el] - 1;                        
                        temp_size = start_size - (double)(result_arr[last_el] * type_size[last_el]);
                    }
                    for (int k = 0; k <= last_el; k++)
                    {
                        type_size[k] = -1;
                    }

                }

                foreach (double t in type_size)
                {
                    if (t > 0)
                    {
                        result_arr[i] = (int)Math.Truncate((temp_size / t));
                        temp_size = temp_size - (result_arr[i] * t);
                    }
                    i++;
                }
                ostatok.Add(temp_size);

                double[] temp = new double[result_arr.Length];
                result_arr.CopyTo(temp, 0);

                flag = false;
                for(int h=result_arr.Length-2;h>=0;h--)
                {
                    if (result_arr[h] > 0) {
                        flag = true;
                        break;
                    }
                }

                result_list.Add(temp);


            }
            while (flag);

            print_array(result_list, ostatok, kolich_d);
        }

        private int get_last_el(double[] result_arr)
        {
            for (int i = result_arr.Length-2; i >=0 ; i--)
            {
                if (result_arr[i] > 0)
                {
                    return i;
                }
            }
            
            return -1;
        }
    }
}
