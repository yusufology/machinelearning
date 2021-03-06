﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.StaticPipe;
using Microsoft.ML.Trainers.Online;
using Xunit;

namespace Microsoft.ML.Tests.TrainerEstimators
{
    public partial class TrainerEstimators
    {
        [Fact]
        public void OnlineLinearWorkout()
        {
            var dataPath = GetDataPath("breast-cancer.txt");

            var data = TextLoader.CreateReader(Env, ctx => (Label: ctx.LoadFloat(0), Features: ctx.LoadFloat(1, 10)))
                .Read(dataPath);

            var pipe = data.MakeNewEstimator()
                .Append(r => (r.Label, Features: r.Features.Normalize()));

            var trainData = pipe.Fit(data).Transform(data).AsDynamic;

            var ogdTrainer = new OnlineGradientDescentTrainer(Env, "Label", "Features");
            TestEstimatorCore(ogdTrainer, trainData);
            var ogdModel = ogdTrainer.Fit(trainData);
            ogdTrainer.Train(trainData, ogdModel.Model);

            var apTrainer = new AveragedPerceptronTrainer(Env, "Label", "Features", lossFunction: new HingeLoss(), advancedSettings: s =>
            {
                s.LearningRate = 0.5f;
            });
            TestEstimatorCore(apTrainer, trainData);

            var apModel = apTrainer.Fit(trainData);
            apTrainer.Train(trainData, apModel.Model);

            Done();

        }
    }
}
