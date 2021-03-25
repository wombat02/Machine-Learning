﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using NN;

class MainClass
{
    static void Main(string[] args)
    { 
        NeuralNetwork net = JsonSerializer.Deserialize<NeuralNetwork>
            ( FileManager.ReadFile("net(1)_100_50.json") );
        TrainingParams tParams = new TrainingParams(iMomentum: 0.75f, iMaxEpoch: 10);
        
        TrainNetwork(net, tParams);
    }

    static void TrainNetwork (NeuralNetwork net, TrainingParams trainingParams)
    {
        // test network before training for baseline accuracy
        float accuracy_trained = 0;
        float accuracy_untrained = testNetwork(net);

        // ask if should log training 
        Console.Clear();
        Console.WriteLine("Log training (y/n)?");
        string input = Console.ReadLine();
        string logFileName = null;
        if (input == "y")
        {
            Console.WriteLine("Enter log file name (.csv): ");
            logFileName = Console.ReadLine();
        }

        Console.Clear();

        // train network
        MNISTOptimiser.TrainNetwork(ref net, trainingParams, logFileName);
        Console.WriteLine("Training done!");

        // re-test trained network
        accuracy_trained = testNetwork(net);
        Console.WriteLine("Untrained: {0:F3}\nTrained: {1:F3}", accuracy_untrained, accuracy_trained);

        // get save file name
        Console.WriteLine("Save network as: ");
        input = Console.ReadLine();

        // save nn
        Console.WriteLine("saving as {0}", input);
        FileManager.SerializeAsJson(net, input);
    }

    static float testNetwork(NeuralNetwork net)
    {
        var images = MNISTReader.ReadTestData();

        float accuracy = 0.0f;
        int count = 1;
        int correct = 0;

        int upd_freq = 1000;

        // test all images
        foreach (var img in images)
        {
            // prep data
            Matrix input = null;
            Matrix target = null;
            MNISTOptimiser.processImg(img, out input, out target);

            // get prediction
            Matrix prediction = net.predict(input);
            float highestOutput = 0.0f;
            int prediction_int = 0;

            for (int i = 0; i < prediction.Rows; i++)
            {
                if (prediction.GetValue(i, 0) > highestOutput)
                {
                    highestOutput = prediction.GetValue(i, 0);
                    prediction_int = i;
                }
            }

            if (prediction_int == img.label)
                correct++;

            if (count % upd_freq == 0)
            {
                Console.WriteLine("{0} / {1}\t|\t{2}", correct, count, ((float)correct / (float)count) * 100);
            }

            count++;
        }

        // calculate avg
        accuracy = (float)correct / (float)count;
        accuracy *= 100;

        return accuracy;
    }

    //METHODS
    static public void VisualiseData()
    {
        int maxNum = 10;
        int counter = 1;

        foreach (var Image in MNISTReader.ReadTrainingData())
        {
            float[] imgInputs = new float[28 * 28];
            float[] imgExpectedOutput = new float[10];

            for (int i = 0; i < 10; i++)
            {
                if (i == Image.label)
                    imgExpectedOutput[i] = 1;
                else
                    imgExpectedOutput[i] = 0;
            }

            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    imgInputs[j + (j * i)] = (float)(Image.data[i, j]);
                }
            }

            Matrix expectedOutput = new Matrix(10, 1, imgExpectedOutput);

            //display img
            string imgStr = "";
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    if ((i == 0 || i == 27) || j == 0)
                        imgStr += "|";

                    char outputChar = 'a';

                    float val = (float)Image.data[i, j] / 255f;

                    if (val < 0.25f)
                        outputChar = '.';
                    else if (val < .5f)
                        outputChar = '*';
                    else
                        outputChar = '#';

                    if (val == 0f)
                        outputChar = ' ';

                    imgStr += outputChar;

                    if (j == 27)
                        imgStr += "|\n";
                }

            }

            Console.WriteLine(imgStr);
            Console.WriteLine(Image.label);
            Console.WriteLine(expectedOutput.ToString());

            if (counter == maxNum)
                break;

            counter++;
        }
    }
}