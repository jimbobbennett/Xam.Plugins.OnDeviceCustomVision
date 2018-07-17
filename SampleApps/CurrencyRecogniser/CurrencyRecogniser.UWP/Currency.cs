using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// 4ccd9122-2eea-4b2e-b072-d7efe77c556f_e9b77d7f-235c-46e3-927b-39b2488e86cc

namespace CurrencyRecogniser.UWP
{
    public sealed class _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> loss { get; set; }
        public _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModelOutput()
        {
            this.classLabel = new List<string>();
            this.loss = new Dictionary<string, float>()
            {
                { "FivePounds", float.NaN },
                { "TenPounds", float.NaN },
            };
        }
    }

    public sealed class _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModel
    {
        private LearningModelPreview learningModel;
        public static async Task<_x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModel> Create_x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModel model = new _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<_x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModelOutput> EvaluateAsync(_x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModelInput input) {
            _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModelOutput output = new _x0034_ccd9122_x002D_2eea_x002D_4b2e_x002D_b072_x002D_d7efe77c556f_e9b77d7f_x002D_235c_x002D_46e3_x002D_927b_x002D_39b2488e86ccModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("loss", output.loss);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
