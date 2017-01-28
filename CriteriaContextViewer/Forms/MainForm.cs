using CriteriaContextViewer.Model.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CriteriaContextViewer.Model;
using CriteriaContextViewer.Model.Readers;

namespace CriteriaContextViewer.Forms
{
    public partial class MainForm : Form
    {
        private XmlDocument m_definitions;

        public Dictionary<short, Scenario> Scenarios { get; set; }
        public Dictionary<short, ScenarioStep> ScenarioSteps { get; set; }
        public Dictionary<uint, CriteriaTree> CriteriaTrees { get; set; }
        public Dictionary<uint, Criteria> Criterias { get; set; }

        public MainForm()
        {
            InitializeComponent();

            Scenarios = new Dictionary<short, Scenario>();
            ScenarioSteps = new Dictionary<short, ScenarioStep>();
            Criterias = new Dictionary<uint, Criteria>();
            CriteriaTrees = new Dictionary<uint, CriteriaTree>();
            m_definitions = new XmlDocument();
            m_definitions.Load(Path.Combine(new Uri(
                new FileInfo(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath)
                    .Directory?
                    .FullName).LocalPath, "dbclayout.xml"));
            LoadScenarios();
        }

        public IEnumerable<T> LoadDBCObject<T>(Type type, string fileName) where T : IDBObjectReader, new()
        {
            string assemblyPath =
                new Uri(
                    new FileInfo(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath)
                        .Directory.FullName).LocalPath;

            string fullFilePath = Path.Combine(assemblyPath, fileName);
            XmlElement definition = GetDefinition(Path.GetFileNameWithoutExtension(fileName),
                Path.GetFileName(fullFilePath));

            IWowClientDBReader dbReader;
            try
            {
                dbReader = DBReaderFactory.GetReader(fullFilePath, definition);
            }
            catch (Exception)
            {
                return new List<T>();
            }

            IList<T> objects = new List<T>();
            foreach (var row in dbReader.Rows)
            {
                T t = new T();
                t.ReadObject(dbReader, row);
                objects.Add(t);
            }

            return objects;
        }

        public void LoadScenarios(List<CriteriaTree> criteriaTrees = null)
        {
            Criterias.Clear();
            Criterias = LoadDBCObject<Criteria>(typeof(Criteria), Criteria.FileName)
                .ToDictionary(criteria => criteria.Id, criteria => criteria);

            CriteriaTrees.Clear();
            CriteriaTrees = LoadDBCObject<CriteriaTree>(typeof(CriteriaTree), CriteriaTree.FileName)
                .ToDictionary(criteriaTree => criteriaTree.Id, criteriaTree => criteriaTree);

            // Setup Criteria Tree links
            foreach (var criteriaTree in CriteriaTrees)
            {
                if (Criterias.ContainsKey(criteriaTree.Value.CriteriaId))
                    criteriaTree.Value.Criteria = Criterias[criteriaTree.Value.CriteriaId];

                if (CriteriaTrees.ContainsKey(criteriaTree.Value.ParentId))
                {
                    criteriaTree.Value.Parent = CriteriaTrees[criteriaTree.Value.ParentId];
                    CriteriaTrees[criteriaTree.Value.ParentId].Children.Add(criteriaTree.Value);
                }
            }

            Scenarios.Clear();
            Scenarios = LoadDBCObject<Scenario>(typeof(Scenario), Scenario.FileName)
                .ToDictionary(scenario => (short) scenario.Id, scenario => scenario);
            
            ScenarioSteps.Clear();
            ScenarioSteps = LoadDBCObject<ScenarioStep>(typeof(ScenarioStep), ScenarioStep.FileName)
                .ToDictionary(scenarioStep => (short) scenarioStep.Id, scenarioStep => scenarioStep);
            
            foreach (var scenarioStep in ScenarioSteps)
            {
                // Setup Scenario links
                if (Scenarios.ContainsKey(scenarioStep.Value.ScenarioId))
                {
                    Scenarios[scenarioStep.Value.ScenarioId].Steps.Add(scenarioStep.Value);
                    scenarioStep.Value.Scenario = Scenarios[scenarioStep.Value.ScenarioId];
                }

                // Setup Scenario Step links
                if (ScenarioSteps.ContainsKey(scenarioStep.Value.PreviousStepId))
                    scenarioStep.Value.PreviousStep =
                        ScenarioSteps[scenarioStep.Value.PreviousStepId];
                if (ScenarioSteps.ContainsKey((short)scenarioStep.Value.BonusObjectiveRequiredStepId))
                    scenarioStep.Value.BonusObjectiveRequiredStep =
                        ScenarioSteps[(short)scenarioStep.Value.BonusObjectiveRequiredStepId];
                if (CriteriaTrees.ContainsKey(scenarioStep.Value.CriteriaTreeId))
                    scenarioStep.Value.CriteriaTree = CriteriaTrees[scenarioStep.Value.CriteriaTreeId];
            }
        }

        private XmlElement GetDefinition(string dbcName, string dbcNameWithExt)
        {
            XmlNodeList definitions = m_definitions["DBFilesClient"].GetElementsByTagName(dbcName);
            
            if (definitions.Count == 0)
            {
                definitions = m_definitions["DBFilesClient"].GetElementsByTagName(dbcNameWithExt);
            }
            
            if (definitions.Count == 1)
                return (XmlElement)definitions[0];

            return null;
        }
    }
}
