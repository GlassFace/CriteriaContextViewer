﻿using CriteriaContextViewer.Model.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CriteriaContextViewer.Model;
using CriteriaContextViewer.Model.Readers;
using CriteriaContextViewer.Properties;
using CriteriaContextViewer.Utils.Misc;

namespace CriteriaContextViewer.Forms
{
    public partial class MainForm : Form
    {
        private readonly XmlDocument _mDefinitions;
        private const string DbcLayoutFileName = @"dbclayout.xml";

        public Dictionary<short, Scenario> Scenarios { get; set; }
        public Dictionary<short, ScenarioStep> ScenarioSteps { get; set; }
        public Dictionary<uint, CriteriaTree> CriteriaTrees { get; set; }
        public Dictionary<uint, Criteria> Criterias { get; set; }

        public string DBCLayoutFilePath => Path.Combine(AssemblyUtils.ExecutingAssemblyPath, DbcLayoutFileName);
        public string ScenarioFilePath => Path.Combine(AssemblyUtils.ExecutingAssemblyPath, Scenario.FileName);
        public string ScenarioStepFilePath => Path.Combine(AssemblyUtils.ExecutingAssemblyPath, ScenarioStep.FileName);
        public string CriteriaFilePath => Path.Combine(AssemblyUtils.ExecutingAssemblyPath, Criteria.FileName);
        public string CriteriaTreeFilePath => Path.Combine(AssemblyUtils.ExecutingAssemblyPath, CriteriaTree.FileName);

        public MainForm()
        {
            InitializeComponent();

            Scenarios = new Dictionary<short, Scenario>();
            ScenarioSteps = new Dictionary<short, ScenarioStep>();
            Criterias = new Dictionary<uint, Criteria>();
            CriteriaTrees = new Dictionary<uint, CriteriaTree>();
            _mDefinitions = new XmlDocument();

            IEnumerable<string> missingFiles = GetMissingFiles();
            if (missingFiles.Any())
            {
                string files = string.Join(", ", missingFiles);
                MessageBox.Show($"One ore more files were not found in the program main directory, please verify the following files exists: \"{files}\"", "Files not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Load += (o, e) => Close();
                return;
            }
            _mDefinitions.Load(DBCLayoutFilePath);

            scenarioSearchBy.DataSource = Enum.GetValues(typeof(ScenarioSearchType));
        }

        public IEnumerable<string> GetMissingFiles()
        {
            List<string> missingFiles = new List<string>();

            if (!File.Exists(DBCLayoutFilePath))
                missingFiles.Add(Path.GetFileName(DBCLayoutFilePath));

            if (!File.Exists(ScenarioFilePath))
                missingFiles.Add(Path.GetFileName(ScenarioFilePath));

            if (!File.Exists(ScenarioStepFilePath))
                missingFiles.Add(Path.GetFileName(ScenarioStepFilePath));

            if (!File.Exists(CriteriaFilePath))
                missingFiles.Add(Path.GetFileName(CriteriaFilePath));

            if (!File.Exists(CriteriaTreeFilePath))
                missingFiles.Add(Path.GetFileName(CriteriaTreeFilePath));

            return missingFiles;
        }

        public IEnumerable<T> LoadDBCObject<T>(Type type, string fileName) where T : IDBObjectReader, new()
        {
            string fullFilePath = Path.Combine(AssemblyUtils.ExecutingAssemblyPath, fileName);
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
            int totalRows = dbReader.Rows.Count();
            int currentRow = 0;
            foreach (var row in dbReader.Rows)
            {
                T t = new T();
                t.ReadObject(dbReader, row);
                objects.Add(t);
            }

            return objects;
        }

        public void LoadCriterias(bool reload = false)
        {
            if (reload)
                Criterias.Clear();

            if (!Criterias.Any())
                Criterias =
                    LoadDBCObject<Criteria>(typeof(Criteria), Criteria.FileName)
                        .ToDictionary(criteria => criteria.Id, criteria => criteria);
        }

        public void LoadCriteriaTrees(bool reload = false)
        {
            LoadCriterias(reload);

            if (reload)
                CriteriaTrees.Clear();

            if (!CriteriaTrees.Any())
                CriteriaTrees =
                    LoadDBCObject<CriteriaTree>(typeof(CriteriaTree), CriteriaTree.FileName)
                        .ToDictionary(criteriaTree => criteriaTree.Id, criteriaTree => criteriaTree);

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

            // Order criteria tree children by criteria tree child index
            foreach (var criteriaTree in CriteriaTrees)
            {
                criteriaTree.Value.Children = criteriaTree.Value.Children.OrderBy(tree => tree.OrderIndex).ToList();
            }
        }

        public void LoadScenarios(bool reload = false)
        {
            LoadCriteriaTrees(reload);

            if (reload)
                Scenarios.Clear();

            if (!Scenarios.Any())
                Scenarios =
                    LoadDBCObject<Scenario>(typeof(Scenario), Scenario.FileName)
                        .ToDictionary(scenario => (short) scenario.Id, scenario => scenario);

            if (reload)
                ScenarioSteps.Clear();

            if (!ScenarioSteps.Any())
                ScenarioSteps =
                    LoadDBCObject<ScenarioStep>(typeof(ScenarioStep), ScenarioStep.FileName)
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

                if (ScenarioSteps.ContainsKey((short) scenarioStep.Value.BonusObjectiveRequiredStepId))
                    scenarioStep.Value.BonusObjectiveRequiredStep =
                        ScenarioSteps[(short) scenarioStep.Value.BonusObjectiveRequiredStepId];

                if (CriteriaTrees.ContainsKey(scenarioStep.Value.CriteriaTreeId))
                    scenarioStep.Value.CriteriaTree = CriteriaTrees[scenarioStep.Value.CriteriaTreeId];
            }

            // Order scenario steps in scenarios by scenario step index
            foreach (var scenario in Scenarios)
            {
                scenario.Value.Steps = scenario.Value.Steps.OrderBy(step => step.StepIndex).ToList();
            }
        }

        private XmlElement GetDefinition(string dbcName, string dbcNameWithExt)
        {
            XmlNodeList definitions = _mDefinitions["DBFilesClient"].GetElementsByTagName(dbcName);

            if (definitions.Count == 0)
            {
                definitions = _mDefinitions["DBFilesClient"].GetElementsByTagName(dbcNameWithExt);
            }

            if (definitions.Count == 1)
                return (XmlElement) definitions[0];

            return null;
        }

        private void tabControl1_Enter(object sender, EventArgs e)
        {
            LoadScenarios();

            listBoxScenarios.ValueMember = "Value";
            listBoxScenarios.DisplayMember = "Display";
            listBoxScenarios.DataSource = Scenarios.Select(scenario => scenario.Value).ToList();

            //backgroundWorker1.DoWork += (o, args) => LoadScenarios();
            //LoadingForm form = new LoadingForm("Loading scenario data", backgroundWorker1);
            //form.Show();
            //backgroundWorker1.RunWorkerAsync();
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            Scenario scenario = (Scenario) listBoxScenarios.SelectedItem;
            if (scenario == null)
                return;

            labelScenarioName.Text = scenario.Name;
            textBoxScenarioId.Text = scenario.Id.ToString();
            textBoxScenarioType.Text = $"{(int) scenario.Type} - {scenario.Type.GetDescription()}";
            listBoxScenarioFlags.DataSource =
                Enum.GetValues(typeof(ScenarioFlags))
                    .Cast<ScenarioFlags>()
                    .Where(flag => scenario.Flags.HasFlag(flag))
                    .Select(flag => flag.GetDescription()).ToList();
            listBoxScenarioSteps.ValueMember = "Value";
            listBoxScenarioSteps.DisplayMember = "Display";
            listBoxScenarioSteps.DataSource = scenario.Steps;
        }

        private void checkBoxScenarioStepBonusObjective_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox) sender;
            if (checkbox == null)
                return;

            labelScenarioStepRequiredStepId.Visible = checkbox.Checked;
            textBoxScenarioStepRequiredStepId.Visible = checkbox.Checked;
        }

        private void listBoxScenarioSteps_SelectedValueChanged(object sender, EventArgs e)
        {
            ScenarioStep step = (ScenarioStep) listBoxScenarioSteps.SelectedItem;
            if (step == null)
                return;

            labelScenarioStepName.Text = step.Name;
            textBoxScenarioStepDescription.Text = step.Description;
            textBoxScenarioStepId.Text = step.Id.ToString();
            textBoxScenarioStepIndex.Text = step.StepIndex.ToString();
            textBoxScenarioStepQuestRewardId.Text = step.QuestRewardId.ToString();
            linkLabelScenarioStepQuestReward.Visible = !(textBoxScenarioStepQuestRewardId.Text == "0" || string.IsNullOrEmpty(textBoxScenarioStepQuestRewardId.Text));
            checkBoxScenarioStepBonusObjective.Checked = (step.Flags & ScenarioStepFlag.BonusObjective) != 0;
        }

        private void buttonSearchScenarios_Click(object sender, EventArgs e)
        {
            ScenarioSearchType searchType = (ScenarioSearchType) scenarioSearchBy.SelectedItem;
            switch (searchType)
            {
                case ScenarioSearchType.ByScenarioName:
                    listBoxScenarios.DataSource =
                        Scenarios.Where(
                                scenario =>
                                    scenario.Value.Name?.ToLower(CultureInfo.InvariantCulture)
                                        .Contains(textBoxSearchScenarios.Text.ToLower(CultureInfo.InvariantCulture)) == true)
                            .Select(scenario => scenario.Value)
                            .ToList();
                    break;
                case ScenarioSearchType.ByScenarioId:
                    int scenarioId;
                    if (!int.TryParse(textBoxSearchScenarios.Text, out scenarioId))
                        break;

                    listBoxScenarios.DataSource =
                        Scenarios.Where(
                            scenario =>
                                scenario.Value.Id == scenarioId).Select(scenario => scenario.Value).ToList();
                    break;
                case ScenarioSearchType.ByScenarioType:
                    int scenarioTypeId;
                    if (!int.TryParse(textBoxSearchScenarios.Text, out scenarioTypeId))
                    {
                        listBoxScenarios.DataSource = SearchByScenarioTypeDescription(textBoxSearchScenarios.Text);
                        break;
                    }

                    ScenarioType type = (ScenarioType) scenarioTypeId;
                    List<Scenario> scenarios = Scenarios.Where(
                        scenario =>
                            scenario.Value.Type == type).Select(scenario => scenario.Value).ToList();

                    if (!scenarios.Any())
                    {
                        listBoxScenarios.DataSource = SearchByScenarioTypeDescription(textBoxSearchScenarios.Text);
                        break;
                    }

                    listBoxScenarios.DataSource = scenarios;
                    break;
                case ScenarioSearchType.ByScenarioStepName:
                    listBoxScenarios.DataSource =
                        Scenarios.Where(
                                scenario =>
                                    scenario.Value.Steps.Any(
                                        step =>
                                            step.Name?.ToLower(CultureInfo.InvariantCulture)
                                                .Contains(
                                                    textBoxSearchScenarios.Text.ToLower(CultureInfo.InvariantCulture)) ==
                                            true))
                            .Select(scenario => scenario.Value)
                            .ToList();
                    break;
                case ScenarioSearchType.ByScenarioStepDescription:
                    listBoxScenarios.DataSource =
                        Scenarios.Where(
                                scenario =>
                                    scenario.Value.Steps.Any(
                                        step =>
                                            step.Description?.ToLower(CultureInfo.InvariantCulture)
                                                .Contains(
                                                    textBoxSearchScenarios.Text.ToLower(CultureInfo.InvariantCulture)) ==
                                            true))
                            .Select(scenario => scenario.Value)
                            .ToList();
                    break;
                case ScenarioSearchType.UsesCriteriaTreeId:
                    uint criteriaTreeId;
                    if (!uint.TryParse(textBoxSearchScenarios.Text, out criteriaTreeId))
                        break;

                    listBoxScenarios.DataSource =
                        Scenarios.Where(
                                scenario =>
                                    scenario.Value.Steps.Where(step => step.CriteriaTree != null).Any(
                                        step =>
                                            step.CriteriaTree.Id == criteriaTreeId ||
                                            CriteriaTreeHasChildCriteriaTreeId(step.CriteriaTree, criteriaTreeId)))
                            .Select(scenario => scenario.Value)
                            .ToList();
                    break;
                case ScenarioSearchType.UsesCriteriaId:
                    uint criteriaId;
                    if (!uint.TryParse(textBoxSearchScenarios.Text, out criteriaId))
                        break;

                    listBoxScenarios.DataSource =
                        Scenarios.Where(
                                scenario =>
                                    scenario.Value.Steps.Where(step => step.CriteriaTree != null).Any(
                                        step =>
                                            step.CriteriaTree.CriteriaId == criteriaId ||
                                            CriteriaTreeHasChildCriteriaId(step.CriteriaTree, criteriaId)))
                            .Select(scenario => scenario.Value)
                            .ToList();
                    break;
                case ScenarioSearchType.HasBonusObjective:
                    listBoxScenarios.DataSource =
                        Scenarios.Where(
                                scenario =>
                                    scenario.Value.Steps.Any(step => (step.Flags & ScenarioStepFlag.BonusObjective) != 0))
                            .Select(scenario => scenario.Value)
                            .ToList();
                    break;
            }
        }

        public bool CriteriaTreeHasChildCriteriaTreeId(CriteriaTree criteriaTree, uint criteriaTreeId)
        {
            return criteriaTree.Children.Any(child => child.Id == criteriaTreeId) ||
                   criteriaTree.Children.Any(child => CriteriaTreeHasChildCriteriaTreeId(child, criteriaTreeId));
        }

        public bool CriteriaTreeHasChildCriteriaId(CriteriaTree criteriaTree, uint criteriaId)
        {
            return criteriaTree.Children.Any(child => child.CriteriaId == criteriaId) ||
                   criteriaTree.Children.Any(child => CriteriaTreeHasChildCriteriaId(child, criteriaId));
        }

        private List<Scenario> SearchByScenarioTypeDescription(string description)
        {
            Dictionary<ScenarioType, string> scenarioTypesWithDescriptions = new Dictionary<ScenarioType, string>();
            Enum.GetValues(typeof(ScenarioType))
                .Cast<ScenarioType>()
                .Select(type => new KeyValuePair<ScenarioType, string>(type, type.GetDescription()))
                .ToList()
                .ForEach(pair => scenarioTypesWithDescriptions.Add(pair.Key, pair.Value));

            List<ScenarioType> typesMatch =
                scenarioTypesWithDescriptions.Where(
                        typePair =>
                            typePair.Value.ToLower(CultureInfo.InvariantCulture)
                                .Contains(description.ToLower(CultureInfo.InvariantCulture)))
                    .Select(typePair => typePair.Key)
                    .ToList();
            return
                Scenarios.Where(scenario => typesMatch.Contains(scenario.Value.Type))
                    .Select(scenario => scenario.Value)
                    .ToList();
        }

        private void textBoxSearchScenarios_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            buttonSearchScenarios.PerformClick();
        }

        private void scenarioSearchBy_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox) sender;
            if (comboBox == null)
                return;

            ScenarioSearchType type = (ScenarioSearchType) comboBox.SelectedItem;
            if (type == ScenarioSearchType.HasBonusObjective)
            {
                textBoxSearchScenarios.Enabled = false;
                buttonSearchScenarios.PerformClick();
            }
            else
                textBoxSearchScenarios.Enabled = true;
        }

        private void buttonInspectCriterias_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Not yet implemented", "NYI", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void linkLabelScenarioStepQuestReward_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(WowheadUtils.GetWowheadURLForQuest(int.Parse(textBoxScenarioStepQuestRewardId.Text)));
        }
    }

    public enum ScenarioSearchType
    {
        [Description("Scenario name")]
        ByScenarioName,
        [Description("Scenario id")]
        ByScenarioId,
        [Description("Scenario type")]
        ByScenarioType,
        [Description("Step name")]
        ByScenarioStepName,
        [Description("Step id")]
        ByScenarioStepId,
        [Description("Step description")]
        ByScenarioStepDescription,
        [Description("Step uses criteria tree id")]
        UsesCriteriaTreeId,
        [Description("Step uses criteria id")]
        UsesCriteriaId,
        [Description("Has bonus objectives")]
        HasBonusObjective,
    }
}