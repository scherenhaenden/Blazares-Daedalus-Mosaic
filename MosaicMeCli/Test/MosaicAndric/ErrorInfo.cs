using System.Collections;
using System.Drawing;

namespace MosaicMeCli.Test.MosaicAndric;

public class IMScatterSearch : ScatterSearch
  {
    private MosaicH m_mosaic_info;
    private ArrayList m_BlockList;
    private Hashtable k_hash;
    private Size m_solution_size;
    private Point[] To2D;
    private ArrayList m_Positions;
    public double[] m_solution_values;
    private bool first_time = true;
    private Hashtable combined_ht = (Hashtable) null;

    private int To1D(int i, int j) => i + this.m_solution_size.Width * j;

    public IMScatterSearch(MosaicH p_mosaic_info, ArrayList p_BlockList)
    {
      this.p_b1 = 3;
      this.p_b2 = 2;
      this.p_S = 6;
      this.p_Max_Iterations = 10;
      this.p_LocalMaxIterations = 3000;
      this.m_mosaic_info = p_mosaic_info;
      this.m_BlockList = p_BlockList;
      this.m_solution_size = BlockTransform.GetBlockMatrixSize(p_BlockList);
      this.k_hash = new Hashtable(8111);
      this.To2D = new Point[this.m_BlockList.Count];
      this.m_Positions = new ArrayList(this.m_BlockList.Count);
      int index = 0;
      for (int y = 0; y < this.m_solution_size.Height; ++y)
      {
        for (int x = 0; x < this.m_solution_size.Width; ++x)
        {
          this.To2D[index] = new Point(x, y);
          this.m_Positions.Add((object) index);
          ++index;
        }
      }
    }

    private IMSolution FindRelaxedSolution()
    {
      IMSolution relaxedSolution = new IMSolution(this.m_solution_size);
      relaxedSolution._value = 0.0;
      for (int index = 0; index < relaxedSolution._x.Length; ++index)
      {
        ImageLink imageLink = this.m_mosaic_info.MatchBlock((ImageBlockInfo) this.m_BlockList[index]);
        relaxedSolution._x[index] = imageLink.MemoryId;
        relaxedSolution.errors[index] = this.K(imageLink.MemoryId, index);
        relaxedSolution._value += relaxedSolution.errors[index];
      }
      relaxedSolution.IsFeasible(this.m_mosaic_info.DataBase);
      return relaxedSolution;
    }

    public override ISolution NewSolution()
    {
      DateTime now = DateTime.Now;
      this.m_mosaic_info.InitializeDB();
      ArrayList arrayList = (ArrayList) this.m_Positions.Clone();
      IMSolution imSolution = new IMSolution(this.m_solution_size);
      imSolution._value = 0.0;
      for (int index1 = 0; index1 < this.m_BlockList.Count; ++index1)
      {
        int index2 = this.SGenerator.Next(this.m_BlockList.Count - 1 - index1);
        int index3 = (int) arrayList[index2];
        arrayList.RemoveAt(index2);
        ImageLink imageLink = this.m_mosaic_info.MatchBlock((ImageBlockInfo) this.m_BlockList[index3]);
        imSolution._x[index3] = imageLink.MemoryId;
        imageLink.usable = false;
        imSolution._value += this.K(imageLink.MemoryId, index3);
      }
      TimeSpan timeSpan = DateTime.Now - now;
      Console.WriteLine("New Solution: value=" + (object) imSolution.Value + " time to compute=" + (object) timeSpan.TotalSeconds);
      if (!imSolution.IsFeasible(this.m_mosaic_info.DataBase))
        Console.WriteLine("not feasible solution!");
      return (ISolution) imSolution;
    }

    private void SetUpReducedList(ImageLink[] m_Reduced)
    {
      int index1 = 0;
      for (int index2 = 0; index2 < this.m_mosaic_info.DataBase.Count; ++index2)
      {
        if (((ImageLink) this.m_mosaic_info.DataBase[index2]).usable)
        {
          m_Reduced[index1] = (ImageLink) this.m_mosaic_info.DataBase[index2];
          ++index1;
        }
      }
    }

    public override ISolution LocalOptimization(ISolution starting_solution)
    {
      this.EvaluateSolution(starting_solution);
      double num1 = starting_solution.Value;
      IMSolution sol = (IMSolution) starting_solution;
      this.m_mosaic_info.InitializeDB();
      this.TagAsUsable(sol._x, false);
      ImageLink[] imageLinkArray = new ImageLink[this.m_mosaic_info.DataBase.Count - sol._x.Length + 1];
      this.SetUpReducedList(imageLinkArray);
      ErrorInfo[] array = new ErrorInfo[this.m_Positions.Count];
      for (int p_i = 0; p_i < array.Length; ++p_i)
        array[p_i] = new ErrorInfo(0.0, p_i);
      int index1 = 0;
      bool flag1 = true;
      for (int index2 = 0; index2 < this.p_LocalMaxIterations; ++index2)
      {
        if (index1 == array.Length)
        {
          for (int index3 = 0; index3 < array.Length; ++index3)
            array[index3].error = this.SGenerator.NextDouble();
          Array.Sort<ErrorInfo>(array);
          flag1 = false;
          index1 = 0;
        }
        int position = array[index1].position;
        ++index1;
        int num2 = sol._x[position];
        bool flag2 = false;
        int index4 = -1;
        int index5 = -1;
        double num3 = 0.0;
        double num4 = this.K(num2, position);
        ImageBlockInfo block = (ImageBlockInfo) this.m_BlockList[position];
        for (int i = 0; i < sol._x.Length; ++i)
        {
          if (i != position)
          {
            double num5 = num4 + this.K(sol._x[i], i);
            ((ImageLink) this.m_mosaic_info.DataBase[sol._x[i]]).usable = true;
            imageLinkArray[imageLinkArray.Length - 1] = (ImageLink) this.m_mosaic_info.DataBase[sol._x[i]];
            ImageLink imageLink = this.m_mosaic_info.MatchBlock(block, imageLinkArray);
            double num6 = this.K(imageLink.MemoryId, position) + this.K(num2, i);
            if (num6 < num5)
            {
              double num7 = -num5 + num6;
              if (num7 < num3)
              {
                flag2 = true;
                num3 = num7;
                index4 = i;
                index5 = imageLink.MemoryId;
                flag1 = true;
              }
            }
            ((ImageLink) this.m_mosaic_info.DataBase[sol._x[i]]).usable = false;
          }
        }
        if (flag2)
        {
          ((ImageLink) this.m_mosaic_info.DataBase[sol._x[index4]]).usable = true;
          ((ImageLink) this.m_mosaic_info.DataBase[index5]).usable = false;
          sol._x[index4] = sol._x[position];
          sol._x[position] = index5;
          this.SetUpReducedList(imageLinkArray);
        }
        if (index1 == array.Length && !flag1)
        {
          Console.WriteLine("Early Exit at iteration " + (object) index2);
          break;
        }
      }
      if (!sol.IsFeasible(this.m_mosaic_info.DataBase))
      {
        Console.WriteLine("not a feasible solution!");
        throw new Exception("Not Feasible!");
      }
      double num8 = sol.Value;
      this.EvaluateSolution((ISolution) sol);
      Console.WriteLine("Local Opt: Start value=" + (object) num1 + " end value=" + (object) sol.Value);
      return (ISolution) sol;
    }

    protected override ISolution Combine(ISolution _s1, ISolution _s2, double alpha) => this.Combine3(_s1, _s2, alpha);

    private ISolution Combine2(ISolution _s1, ISolution _s2, double alpha)
    {
      IMSolution imSolution = (IMSolution) _s1;
      IMSolution s1 = (IMSolution) _s2;
      IMSolution s2 = new IMSolution(this.m_solution_size);
      int distance = (int) imSolution.GetDistance((ISolution) s1);
      ItemPosition[] array1 = new ItemPosition[distance];
      ItemPosition[] array2 = new ItemPosition[distance];
      ArrayList arrayList1 = new ArrayList(distance);
      int index1 = 0;
      for (int p_position = 0; p_position < imSolution._x.Length; ++p_position)
      {
        if (imSolution._x[p_position] != s1._x[p_position])
        {
          arrayList1.Add((object) p_position);
          array1[index1] = new ItemPosition(imSolution._x[p_position], p_position);
          array2[index1] = new ItemPosition(s1._x[p_position], p_position);
          ++index1;
        }
        else
          s2._x[p_position] = imSolution._x[p_position];
      }
      Array.Sort<ItemPosition>(array1);
      Array.Sort<ItemPosition>(array2);
      ArrayList arrayList2 = new ArrayList();
      ArrayList arrayList3 = new ArrayList();
      for (int index2 = 0; index2 < arrayList1.Count; ++index2)
      {
        int index3 = (int) arrayList1[index2];
        s2._x[index3] = imSolution._x[index3];
      }
      if (true)
      {
        for (int index4 = 0; index4 < array1.Length; ++index4)
        {
          int index5 = (int) arrayList1[index4];
          int p_item = s1._x[index5];
          if (Array.BinarySearch<ItemPosition>(array1, new ItemPosition(p_item)) < 0)
          {
            arrayList2.Add((object) index5);
            arrayList3.Add((object) index4);
          }
        }
        Console.WriteLine("found " + (object) arrayList2.Count + " elements of S2 not included in S1");
        for (int index6 = 0; index6 < arrayList2.Count; ++index6)
        {
          int index7 = (int) arrayList2[index6];
          double error1 = imSolution.errors[index7];
          double error2 = s1.errors[index7];
          s2._x[index7] = s1._x[index7];
        }
      }
      ArrayList arrayList4 = new ArrayList();
      ArrayList arrayList5 = new ArrayList();
      ArrayList arrayList6 = new ArrayList();
      bool[] flagArray = new bool[arrayList1.Count];
      for (int index8 = 0; index8 < arrayList1.Count; ++index8)
        flagArray[index8] = false;
      for (int index9 = 0; index9 < arrayList3.Count; ++index9)
        flagArray[(int) arrayList3[index9]] = true;
      for (int index10 = 0; index10 < arrayList1.Count; ++index10)
      {
        if (!flagArray[index10])
        {
          int index11 = (int) arrayList1[index10];
          int p_item1 = imSolution._x[index11];
          if (Array.BinarySearch<ItemPosition>(array2, new ItemPosition(p_item1)) > 0)
          {
            int p_item2 = s1._x[index11];
            int index12 = Array.BinarySearch<ItemPosition>(array1, new ItemPosition(p_item2));
            if (index12 > 0)
            {
              arrayList4.Add((object) index11);
              arrayList5.Add((object) p_item1);
              arrayList6.Add((object) p_item2);
              flagArray[index10] = true;
              ItemPosition itemPosition = array1[index12];
              arrayList4.Add((object) itemPosition.position);
              arrayList5.Add((object) imSolution._x[itemPosition.position]);
              arrayList6.Add((object) s1._x[itemPosition.position]);
              flagArray[index12] = true;
            }
          }
          else
          {
            arrayList4.Add((object) index11);
            arrayList5.Add((object) p_item1);
            arrayList6.Add((object) p_item1);
            flagArray[index10] = true;
          }
          arrayList5.Sort();
          arrayList6.Sort();
          if ((double) arrayList5.Count / (double) imSolution._x.Length > 0.1)
            break;
        }
      }
      for (int index13 = 0; index13 < arrayList4.Count; ++index13)
      {
        int index14 = (int) arrayList4[index13];
        s2._x[index14] = s1._x[index14];
      }
      this.GetFeasibility(ref s2);
      this.EvaluateSolution((ISolution) s2);
      if (!s2.IsFeasible(this.m_mosaic_info.DataBase))
      {
        s2.GetUnfeasibleBlocks(this.m_mosaic_info.DataBase);
        throw new Exception("Infeasible solution!");
      }
      Console.WriteLine(" f=" + (object) s2.Value + " (1=" + (object) imSolution.Value + " 2=" + (object) s1.Value + " )");
      return (ISolution) s2;
    }

    private void SetUsable(int j, bool status) => ((ImageLink) this.m_mosaic_info.DataBase[j]).usable = status;

    private void AddItem(int _key, int _value)
    {
      this.combined_ht.Add((object) _key, (object) _value);
      this.SetUsable(_key, false);
    }

    private void RemoveItem(int _key)
    {
      this.combined_ht.Remove((object) _key);
      this.SetUsable(_key, true);
    }

    private ISolution Combine3(ISolution _s1, ISolution _s2, double alpha)
    {
      IMSolution imSolution = (IMSolution) _s1;
      IMSolution s = (IMSolution) _s2;
      IMSolution sol = new IMSolution(this.m_solution_size);
      int distance = (int) imSolution.GetDistance((ISolution) s);
      ItemPosition[] array1 = new ItemPosition[distance];
      ItemPosition[] array2 = new ItemPosition[distance];
      ArrayList arrayList = new ArrayList(distance);
      int index1 = 0;
      for (int p_position = 0; p_position < imSolution._x.Length; ++p_position)
      {
        if (imSolution._x[p_position] != s._x[p_position])
        {
          arrayList.Add((object) p_position);
          array1[index1] = new ItemPosition(imSolution._x[p_position], p_position);
          array2[index1] = new ItemPosition(s._x[p_position], p_position);
          ++index1;
        }
        else
          sol._x[p_position] = imSolution._x[p_position];
      }
      Array.Sort<ItemPosition>(array1);
      Array.Sort<ItemPosition>(array2);
      for (int index2 = 0; index2 < arrayList.Count; ++index2)
      {
        int index3 = (int) arrayList[index2];
        sol._x[index3] = imSolution._x[index3];
      }
      this.combined_ht = new Hashtable(117);
      this.m_mosaic_info.InitializeDB();
      for (int index4 = 0; index4 < imSolution._x.Length; ++index4)
        this.AddItem(imSolution._x[index4], index4);
      int num1 = (int) (alpha * (double) arrayList.Count);
      ItemBlocks[] unfeasibleBlocks;
      for (int index5 = 0; index5 < num1; ++index5)
      {
        int index6 = (int) arrayList[index5];
        int num2 = s._x[index6];
        object obj = this.combined_ht[(object) num2];
        if (obj == null)
        {
          this.RemoveItem(sol._x[index6]);
          sol._x[index6] = num2;
          this.AddItem(num2, index6);
        }
        else
        {
          int index7 = (int) obj;
          this.RemoveItem(num2);
          this.RemoveItem(sol._x[index6]);
          sol._x[index6] = num2;
          this.AddItem(num2, index6);
          ImageLink imageLink = this.m_mosaic_info.MatchBlock((ImageBlockInfo) this.m_BlockList[index7]);
          sol._x[index7] = imageLink.MemoryId;
          unfeasibleBlocks = sol.GetUnfeasibleBlocks(this.m_mosaic_info.DataBase);
          this.AddItem(imageLink.MemoryId, index7);
        }
      }
      this.EvaluateSolution((ISolution) sol);
      if (!sol.IsFeasible(this.m_mosaic_info.DataBase))
      {
        unfeasibleBlocks = sol.GetUnfeasibleBlocks(this.m_mosaic_info.DataBase);
        throw new Exception("Infeasible solution!");
      }
      Console.WriteLine(" f=" + (object) sol.Value + " (1=" + (object) imSolution.Value + " 2=" + (object) s.Value + " )");
      return (ISolution) sol;
    }

    private void GetFeasibility(ref IMSolution s)
    {
      this.m_mosaic_info.InitializeDB();
      ItemBlocks[] unfeasibleBlocks = s.GetUnfeasibleBlocks(this.m_mosaic_info.DataBase);
      this.TagAsUsable(s._x, false);
      ArrayList arrayList = new ArrayList();
      for (int index1 = 0; index1 < unfeasibleBlocks.Length; ++index1)
      {
        ErrorInfo[] array = new ErrorInfo[unfeasibleBlocks[index1].positions.Count];
        for (int index2 = 0; index2 < unfeasibleBlocks[index1].positions.Count; ++index2)
        {
          int position = (int) unfeasibleBlocks[index1].positions[index2];
          array[index2] = new ErrorInfo(this.K(unfeasibleBlocks[index1].db_item, position), position);
        }
        Array.Sort<ErrorInfo>(array);
        for (int index3 = 1; index3 < array.Length; ++index3)
          arrayList.Add((object) array[index3]);
      }
      ErrorInfo[] array1 = new ErrorInfo[arrayList.Count];
      for (int index = 0; index < arrayList.Count; ++index)
        array1[index] = (ErrorInfo) arrayList[index];
      Array.Sort<ErrorInfo>(array1);
      do
      {
        double num1 = double.MaxValue;
        int index4 = -1;
        ImageLink imageLink1 = (ImageLink) null;
        for (int index5 = 0; index5 < arrayList.Count; ++index5)
        {
          ErrorInfo errorInfo = (ErrorInfo) arrayList[index5];
          ImageLink imageLink2 = this.m_mosaic_info.MatchBlock((ImageBlockInfo) this.m_BlockList[errorInfo.position]);
          double num2 = this.K(imageLink2.MemoryId, errorInfo.position);
          if (num2 < num1)
          {
            index4 = index5;
            num1 = num2;
            imageLink1 = imageLink2;
          }
        }
        ErrorInfo errorInfo1 = (ErrorInfo) arrayList[index4];
        s._x[errorInfo1.position] = imageLink1.MemoryId;
        imageLink1.usable = false;
        arrayList.RemoveAt(index4);
      }
      while (arrayList.Count > 0);
    }

    private void TagAsNotUsableExpect(int _i, int _j, int[,] x)
    {
      for (int index1 = 0; index1 < x.GetLength(0); ++index1)
      {
        for (int index2 = 0; index2 < x.GetLength(1); ++index2)
        {
          if (index1 != _i && index2 != _j)
            ((ImageLink) this.m_mosaic_info.DataBase[x[index1, index2]]).usable = false;
        }
      }
    }

    private void TagAsUsable(int[] x, bool p_usable)
    {
      for (int index = 0; index < x.Length; ++index)
        ((ImageLink) this.m_mosaic_info.DataBase[x[index]]).usable = p_usable;
    }

    public override void EvaluateSolution(ISolution sol)
    {
      IMSolution imSolution = (IMSolution) sol;
      double num = 0.0;
      for (int i = 0; i < imSolution._x.Length; ++i)
      {
        imSolution.errors[i] = this.K(imSolution._x[i], i);
        num += imSolution.errors[i];
      }
      imSolution.Value = num;
    }

    internal double K(int item, int i)
    {
      KArgument key = new KArgument(item, i);
      object obj = this.k_hash[(object) key];
      if (obj != null)
        return (double) obj;
      double num = ((ImageBlockInfo) this.m_BlockList[i]).Distance(((ImageLink) this.m_mosaic_info.DataBase[item]).m_info, double.MaxValue);
      this.k_hash.Add((object) key, (object) num);
      return num;
    }

    private void WriteSolutionsValues()
    {
      StreamWriter text = File.CreateText("C:\\imosaic_solutions_values.txt");
      for (int index = 0; index < this.m_solution_values.Length; ++index)
        text.WriteLine(this.m_solution_values[index].ToString());
      text.Close();
    }

    public IMSolution GenerateSolutions(int n)
    {
      IMSolution[] imSolutionArray = new IMSolution[n];
      this.m_solution_values = new double[imSolutionArray.Length];
      for (int index = 0; index < n; ++index)
      {
        imSolutionArray[index] = (IMSolution) this.NewSolution();
        this.m_solution_values[index] = imSolutionArray[index].Value;
      }
      this.WriteSolutionsValues();
      return imSolutionArray[0];
    }

    public IMSolution Minimize_MultiStart(int n, int max_local_iterations)
    {
      DateTime now = DateTime.Now;
      IMSolution[] imSolutionArray = new IMSolution[n];
      for (int index = 0; index < n; ++index)
      {
        imSolutionArray[index] = (IMSolution) this.NewSolution();
        this.p_LocalMaxIterations = max_local_iterations;
        imSolutionArray[index] = (IMSolution) this.LocalOptimization((ISolution) imSolutionArray[index]);
      }
      Array.Sort<IMSolution>(imSolutionArray);
      this.CalculateDistanceMatrix((ISolution[]) imSolutionArray);
      Console.WriteLine("Optimal Solution Value=" + (object) imSolutionArray[0].Value);
      Console.WriteLine("Running time=" + (object) (DateTime.Now - now).TotalSeconds);
      this.m_solution_values = new double[imSolutionArray.Length];
      for (int index = 0; index < imSolutionArray.Length; ++index)
        this.m_solution_values[index] = imSolutionArray[index].Value;
      this.WriteSolutionsValues();
      return imSolutionArray[0];
    }

    private double[,] CalculateCostMatrix()
    {
      int count1 = this.m_mosaic_info.DataBase.Count;
      int count2 = this.m_BlockList.Count;
      double[,] costMatrix = new double[count2, count1];
      for (int i = 0; i < count2; ++i)
      {
        Console.WriteLine("calculating i=" + (object) i);
        for (int index = 0; index < count1; ++index)
          costMatrix[i, index] = this.K(index, i);
      }
      return costMatrix;
    }

    public IMSolution ImportAMPLSolution(string filename)
    {
      string end = File.OpenText(filename).ReadToEnd();
      IMSolution imSolution = new IMSolution(this.m_solution_size);
      int num1 = this.m_solution_size.Height * this.m_solution_size.Width;
      int num2 = 0;
      for (int index1 = 0; index1 < num1; ++index1)
      {
        for (int index2 = 0; index2 < this.m_mosaic_info.DataBase.Count; ++index2)
        {
          if (int.Parse(new string(end[2 * num2], 1)) == 1)
            imSolution._x[index1] = index2;
          ++num2;
        }
      }
      return imSolution;
    }

    public bool ExportAMPLDatFile(string filename)
    {
      double[,] costMatrix = this.CalculateCostMatrix();
      StreamWriter text = File.CreateText(filename);
      int length1 = costMatrix.GetLength(0);
      int length2 = costMatrix.GetLength(1);
      text.WriteLine("param I:=" + (object) length1 + ";");
      text.WriteLine("param J:=" + (object) length2 + ";");
      text.WriteLine("param c:=");
      for (int index1 = 0; index1 < length1; ++index1)
      {
        Console.WriteLine("writing i=" + (object) index1);
        for (int index2 = 0; index2 < length2; ++index2)
          text.WriteLine((index1 + 1).ToString() + "\t" + (object) (index2 + 1) + "\t" + (object) costMatrix[index1, index2]);
      }
      text.WriteLine(";");
      text.Close();
      return true;
    }
  }

public class ErrorInfo : IComparable
{
    public double error;
    public int position;

    public ErrorInfo(double p_error, int p_i)
    {
        this.error = p_error;
        this.position = p_i;
    }

    public int CompareTo(object obj)
    {
        ErrorInfo errorInfo = (ErrorInfo) obj;
        if (this.error < errorInfo.error)
            return -1;
        return this.error > errorInfo.error ? 1 : 0;
    }
}