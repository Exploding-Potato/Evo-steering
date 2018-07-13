using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

public class NeuralNet
{
	Layer[] layers;

	public NeuralNet (int[] layerSizes, Func<float, float> activationFunction)
	{
		// "First" layer isn't a Layer object
		// So layers has only layerSizes.Count-1 elements
		layers = new Layer[layerSizes.Length - 1];
		for (int i = 0; i < layers.Length; i++)
		{
			layers[i] = new Layer(
				// To account for the bias unit
				layerSizes[i] + 1,
				layerSizes[i + 1],
				activationFunction);
		}
	}

	public NeuralNet (NeuralNet other)
	{
		layers = new Layer[other.layers.Length];
		for (int i = 0; i < layers.Length; i++)
			layers[i] = new Layer(other.layers[i]);
	}

	public float Sum()
	{
		float sum = 0;
		foreach (Layer layer in layers)
			sum += layer.Sum();
		return sum;
	}

	public Vector<float> Predict(Vector<float> input)
	{
		foreach (var layer in layers)
		{
			// Network automatically appends the bias unit
			input = Vector<float>.Build.DenseOfEnumerable(input.Append(1));
			input = layer.ProcessArray(input);
		}

		return input;
	}

	public float[] Predict(float[] input)
	{
		return Predict(Vector<float>.Build.DenseOfArray(input))
			.ToArray();
	}

	public static NeuralNet Mutate(NeuralNet nnet, float magnitude)
	{
		for (int i = 0; i < nnet.layers.Length; i ++)
			nnet.layers[i] = Layer.Mutate(nnet.layers[i], magnitude);

		return nnet;
	}

	public static NeuralNet Recombnine(NeuralNet nnet1, NeuralNet nnet2)
	{
		for (int i = 0; i < nnet1.layers.Length; i ++)
		{
			if (nnet1.layers[i].Inputs != nnet2.layers[i].Inputs ||
				nnet1.layers[i].Outputs != nnet2.layers[i].Outputs)
				throw new ArgumentException("Recombined nnets are differentely sized");

			nnet1.layers[i] = Layer.Recombnine(nnet1.layers[i], nnet2.layers[i]);
		}

		return nnet1;
	}
}