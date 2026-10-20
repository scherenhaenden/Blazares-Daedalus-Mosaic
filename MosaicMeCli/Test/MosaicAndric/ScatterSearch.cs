using System.Collections;

namespace MosaicMeCli.Test.MosaicAndric;

public abstract class ScatterSearch
  {
    public ISolution[] RefSet1;
    public ISolution[] RefSet2;
    public ISolution[] RefSet;
    private ISolution[] OldRefSet1;
    public int p_S = 50;
    public int p_b1 = 10;
    public int p_b2 = 10;
    public int p_Max_Iterations = 15;
    public int p_LocalMaxIterations = 20;
    protected Random SGenerator;

    public ScatterSearch() => this.SGenerator = new Random();

    public abstract ISolution NewSolution();

    public abstract ISolution LocalOptimization(ISolution starting_solution);

    public abstract void EvaluateSolution(ISolution sol);

    protected abstract ISolution Combine(ISolution _s1, ISolution _s2, double alpha);

    public ISolution Minimize()
    {
      DateTime now = DateTime.Now;
      this.RefSet1 = new ISolution[this.p_b1];
      this.RefSet2 = new ISolution[this.p_b2];
      this.RefSet = new ISolution[this.p_b1 + this.p_b2];
      ISolution[] start_solution_set = new ISolution[this.p_S];
      for (int index = 0; index < this.p_S; ++index)
        start_solution_set[index] = this.NewSolution();
      for (int index = 0; index < this.p_S; ++index)
        start_solution_set[index] = this.LocalOptimization(start_solution_set[index]);
      this.OldRefSet1 = (ISolution[]) null;
      for (int index1 = 0; index1 < this.p_Max_Iterations; ++index1)
      {
        this.BuildReferenceSet(start_solution_set);
        bool flag = false;
        foreach (ISolution combineSolution in this.CombineSolutions())
        {
          ISolution s = this.LocalOptimization(combineSolution);
          double diversity = this.GetDiversity(s, (ICollection) this.RefSet);
          if (diversity > 0.0)
          {
            if (s.Value < this.RefSet1[this.p_b1 - 1].Value)
            {
              this.RefSet1[this.p_b1 - 1] = s;
              Array.Sort<ISolution>(this.RefSet1);
              this.RefSetUnion();
              flag = true;
            }
            else
            {
              DoubleKey[] array = new DoubleKey[this.RefSet2.Length];
              for (int p_Info = 0; p_Info < this.RefSet2.Length; ++p_Info)
                array[p_Info] = new DoubleKey(this.GetDiversity(this.RefSet2[p_Info], (ICollection) this.RefSet), (object) p_Info);
              Array.Sort<DoubleKey>(array);
              if (diversity > array[0].Value)
                this.RefSet2[(int) array[0].Info] = s;
            }
          }
        }
        Console.WriteLine("Iteration=" + (object) index1);
        Console.WriteLine("Elite Solutions");
        for (int index2 = 0; index2 < this.RefSet1.Length; ++index2)
          Console.WriteLine("S" + (object) index2 + ":\t" + this.RefSet1[index2].ToString());
        Console.WriteLine("Diverse Solutions");
        for (int index3 = 0; index3 < this.RefSet2.Length; ++index3)
          Console.WriteLine("S" + (object) index3 + ":\t" + this.RefSet2[index3].ToString());
        if (!flag)
        {
          Console.WriteLine("Early Exiting at iteration= " + (object) index1 + ": elite solutions not changed!");
          break;
        }
        Console.WriteLine("");
        for (int index4 = 0; index4 < this.p_b1; ++index4)
          start_solution_set[index4] = this.RefSet1[index4];
        for (int pB1 = this.p_b1; pB1 < this.p_S; ++pB1)
          start_solution_set[pB1] = this.NewSolution();
        for (int pB1 = this.p_b1; pB1 < this.p_S; ++pB1)
          start_solution_set[pB1] = this.LocalOptimization(start_solution_set[pB1]);
      }
      Console.WriteLine("Optimal Solution Value=" + (object) this.RefSet1[0]);
      Console.WriteLine("Running time=" + (object) (DateTime.Now - now).TotalSeconds);
      return this.RefSet1[0];
    }

    protected double[,] CalculateDistanceMatrix(ISolution[] solution_set)
    {
      double[,] distanceMatrix = new double[solution_set.Length, solution_set.Length];
      for (int index1 = 0; index1 < solution_set.Length; ++index1)
      {
        for (int index2 = index1 + 1; index2 < solution_set.Length; ++index2)
          distanceMatrix[index1, index2] = solution_set[index1].GetDistance(solution_set[index2]);
      }
      return distanceMatrix;
    }

    protected void BuildReferenceSet(ISolution[] start_solution_set)
    {
      Array.Sort<ISolution>(start_solution_set);
      ArrayList ref_set = new ArrayList();
      ArrayList candidate_set = new ArrayList();
      for (int index = 0; index < start_solution_set.Length; ++index)
        candidate_set.Add((object) start_solution_set[index]);
      int index1 = 0;
      for (int index2 = 0; index2 < start_solution_set.Length; ++index2)
      {
        if (this.GetDiversity(start_solution_set[index2], (ICollection) this.RefSet1) > 0.0)
        {
          this.RefSet1[index1] = start_solution_set[index2];
          ref_set.Add((object) this.RefSet1[index1]);
          ++index1;
          if (index1 == this.p_b1)
            break;
        }
      }
      for (int index3 = 0; index3 < this.RefSet1.Length; ++index3)
        candidate_set.Remove((object) this.RefSet1[index3]);
      int index4 = 0;
      do
      {
        if (candidate_set.Count > 0)
        {
          int max_arg = 0;
          if (this.GetMaxDiversity(ref_set, candidate_set, out max_arg) > 0.0)
          {
            this.RefSet2[index4] = (ISolution) candidate_set[max_arg];
            ref_set.Add((object) this.RefSet2[index4]);
            ++index4;
            candidate_set.RemoveAt(max_arg);
          }
          else
            break;
        }
      }
      while (index4 < this.p_b2 && candidate_set.Count > 0);
      this.RefSetUnion();
    }

    private void RefSetUnion()
    {
      for (int index = 0; index < this.p_b1; ++index)
        this.RefSet[index] = this.RefSet1[index];
      ISolution solution = (ISolution) null;
      for (int index = 0; index < this.p_b2; ++index)
      {
        if (this.RefSet2[index] == null)
          this.RefSet2[index] = solution;
        else
          solution = this.RefSet2[index];
        this.RefSet[this.p_b1 + index] = this.RefSet2[index];
      }
    }

    protected double GetMaxDiversity(ArrayList ref_set, ArrayList candidate_set, out int max_arg)
    {
      double maxDiversity = 0.0;
      max_arg = -1;
      for (int index = 0; index < candidate_set.Count; ++index)
      {
        double diversity = this.GetDiversity((ISolution) candidate_set[index], (ICollection) ref_set);
        if (diversity > maxDiversity)
        {
          maxDiversity = diversity;
          max_arg = index;
        }
      }
      return maxDiversity;
    }

    protected ISolution[] CombineSolutions()
    {
      ArrayList l = new ArrayList();
      for (int index1 = 0; index1 < this.RefSet.Length; ++index1)
      {
        for (int index2 = index1 + 1; index2 < this.RefSet.Length; ++index2)
        {
          double alpha = this.SGenerator.NextDouble();
          ISolution solution = this.Combine(this.RefSet[index1], this.RefSet[index2], alpha);
          if (solution != null)
            l.Add((object) solution);
        }
      }
      return this.ToArray(l);
    }

    protected ISolution[] ToArray(ArrayList l)
    {
      ISolution[] array = new ISolution[l.Count];
      for (int index = 0; index < l.Count; ++index)
        array[index] = (ISolution) l[index];
      return array;
    }

    private double GetDiversity(ISolution s, ICollection solutions)
    {
      double diversity = double.MaxValue;
      foreach (ISolution solution in (IEnumerable) solutions)
      {
        if (solution != null && !object.ReferenceEquals((object) s, (object) solution))
        {
          double distance = s.GetDistance(solution);
          if (distance < diversity)
            diversity = distance;
        }
      }
      return diversity;
    }
  }