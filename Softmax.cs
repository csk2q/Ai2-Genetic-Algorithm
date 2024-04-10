namespace Ai2_Genetic_Algorithm;

public class Softmax
{
    //Taken from https://github.com/dotnet/machinelearning/blob/07eb68171f1b6e32e8903e4d9db7fb2d5b9b9bac/src/Microsoft.ML.Core/Utilities/MathUtils.cs#L203

    private const double LogTolerance = 30;

    /// <summary>
    /// computes the "softmax" function: log sum_i exp x_i
    /// </summary>
    /// <param name="inputs">Span of numbers to softmax</param>
    /// <returns>the softmax of the numbers</returns>
    /// <remarks>may have slightly lower roundoff error if inputs are sorted, smallest first</remarks>
    public static double SoftMax(ReadOnlySpan<double> inputs)
    {
        int maxIdx = 0;
        double max = double.NegativeInfinity;
        for (int i = 0; i < inputs.Length; i++)
        {
            if (inputs[i] > max)
            {
                maxIdx = i;
                max = inputs[i];
            }
        }

        if (double.IsNegativeInfinity(max))
            return double.NegativeInfinity;

        if (inputs.Length == 1)
            return max;

        double intermediate = 0.0;
        double cutoff = max - LogTolerance;

        for (int i = 0; i < inputs.Length; i++)
        {
            if (i == maxIdx)
                continue;
            if (inputs[i] > cutoff)
                intermediate += Math.Exp(inputs[i] - max);
        }

        if (intermediate > 0.0)
            return (double)(max + Math.Log(1.0 + intermediate));
        return max;
    }

    /// <summary>
    /// computes "softmax" function of two arguments: log (exp x + exp y)
    /// </summary>
    public static double SoftMax(double lx, double ly)
    {
        double max;
        double negDiff;
        if (lx > ly)
        {
            max = lx;
            negDiff = ly - lx;
        }
        else
        {
            max = ly;
            negDiff = lx - ly;
        }

        if (double.IsNegativeInfinity(max) || negDiff < -LogTolerance)
        {
            return max;
        }
        else
        {
            return (double)(max + Math.Log(1.0 + Math.Exp(negDiff)));
        }
    }
}