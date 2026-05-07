using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.AIGC;

#region AICallConfig

/// <summary>
/// Configuration for AI call operations.
/// </summary>
public class AICallConfig
{

}

#endregion

#region AIClissifierConfig
/// <summary>
/// Configuration for AI classification operations, including prompts for various classifiers.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "AIClissifierConfig", Description = "AI Classifier Config", Icon = "*CoreIcon|Classify")]
public class AIClissifierConfig : SObjectController
{
    public const string PROMPT_CLASSIFY_ASSISTANT = @"
Please help me to select an assistant based on the user's needs:
{0}

<Assistant>Not-Found</Assistant>
User request is not recognized.

Please select one of the templates based on the user's needs. 
If the user's request is ambiguous, please select the template that is most likely to meet the user's needs.
If the user's request is not in the list, please select the Not-Found template.
If the user's request is not recognized, please select the Not-Found template.

IMPORTANT: Export the full inner text in the tag <Assistant>, and nothing else.
";

    public const string PROMPT_CLASSIFY_MAIN = @"
Please classify the user's input into one of the following operation categories:

Create, design, edit, add, delete data and objects -> Operation
Add, edit, delete enums and structures -> Operation
Adjust properties -> Operation
Operations or queries on knowledge base -> Knowledge
Request to search, retrieve, get, select an object from database -> Database
Request detailed answer, description, summary for a question -> Ask
Other unknown cases -> Ask

Please select the closest match, return an English word, no explanation needed.";

    public const string PROMPT_CLASSIFY_DOC = @"
Please classify the user's input into one of the following operation categories:

Request to create structure, abstract structure, enum structure, data, node -> Create
Request to create from knowledge base, based on knowledge base content -> Create
Request to add field, parameter, enum value, array element -> Update
Add, modify based on existing structure, enum, data, node -> Update
Add, delete, edit existing enum, structure -> Update
Add, edit, delete on knowledge base -> Knowledge
Request help to search, retrieve, get, select an object -> Query
Request detailed answer, description, summary for a question -> Ask
Other cases -> Ask

Please select the closest match, return an English word, no explanation needed.";

    public const string PROMPT_CLASSIFY_RAG = @"
Please classify the user's input into one of the following operation categories:

Create knowledge base -> Create
Update knowledge base -> Update
Help get, select an object -> Get
Get detailed answer, description, summary for question -> Ask
Other cases -> Ask

Please select the closest match, return an English word, no explanation needed.";

    public const string PROMPT_CLASSIFY_QUERY_SCOPE = @"
Please classify the user's input into one of the following query scopes:

Query document summary -> Summary
Query all content in document -> Document
Query summary of a section in document -> Overview
Query all content of a section in document -> Content

Please select the closest match, return an English word, no explanation needed.";

    public const string PROMPT_CLASSIFY_GENERATE_MULTIPLE = @"
Please classify the user's input into one of the following quantity ranges:

Generate one/single object -> Single
Generate multiple objects -> Multiple

Please select the closest match, return an English word, no explanation needed.";

    public const string PROMPT_CLASSIFY_GENERATE_SOURCE = @"
Please classify the user's input into one of the following source types:

Generate based on user requirements -> Manual
Generate based on knowledge base -> Knowledge

Please select the closest match, return an English word, no explanation needed.";

    public const string PRMOPT_CLASSIFY_CORRELATION = "Compare two pieces of text from the user, output the correlation value between them, range is [0, 1], no explanation needed.";

    public const string PROMPT_HAS_SELECTION = "User currently has an edit object selected";
    public const string PROMPT_NO_SELECTION = "User currently has no edit object selected";

    public const string PROMPT_COMMON_RELATIONSHIP = @"
1. Basic relationships, for example:
is a
belongs to
2. Part-whole relationships, for example:
contains
is part of
3. Attribute relationships, for example:
has
comes from
4. Role and function relationships, for example:
used for
the purpose is
5. Spatial relationships, for example:
located at
adjacent to
6. Temporal relationships, for example:
occurs at
starts at
7. Causal relationships, for example:
caused by
due to
8. Social relationships, for example:
knows
is friends with
is enemies with
9. Logical and reasoning relationships, for example:
equivalent to
opposite to
10. Data relationships, for example:
greater than
less than
11. Predicate relationships, for example:
created
destroyed
12. Other relationships mentioned in the article.
";

    /// <summary>
    /// Prompt for classifying which assistant to use.
    /// </summary>
    public TextBlockProperty PromptAssistantClassifier { get; }
        = new(nameof(PromptAssistantClassifier), "Assistant Classification", PROMPT_CLASSIFY_ASSISTANT, autoFillDefault: true);

    /// <summary>
    /// Prompt for classifying the main operation category.
    /// </summary>
    public TextBlockProperty PromptMainClassifier { get; }
        = new(nameof(PromptMainClassifier), "Main Operation Classification", PROMPT_CLASSIFY_MAIN, autoFillDefault: true);

    /// <summary>
    /// Prompt for classifying document operations.
    /// </summary>
    public TextBlockProperty PromptDocumentClassifier { get; }
        = new(nameof(PromptDocumentClassifier), "Document Operation Classification", PROMPT_CLASSIFY_DOC, autoFillDefault: true);

    /// <summary>
    /// Prompt for classifying knowledge base operations.
    /// </summary>
    public TextBlockProperty PromptKnowledgeClassifier { get; }
        = new(nameof(PromptKnowledgeClassifier), "Knowledge Base Operation Classification", PROMPT_CLASSIFY_RAG, autoFillDefault: true);

    /// <summary>
    /// Prompt for classifying query scope.
    /// </summary>
    public TextBlockProperty PromptQueryScopeClassifier { get; }
        = new(nameof(PromptQueryScopeClassifier), "Query Scope Classification", PROMPT_CLASSIFY_QUERY_SCOPE, autoFillDefault: true);

    /// <summary>
    /// Prompt for classifying whether to generate single or multiple objects.
    /// </summary>
    public TextBlockProperty PromptGenerateMultipleClassifier { get; }
        = new(nameof(PromptGenerateMultipleClassifier), "Multiple Classification", PROMPT_CLASSIFY_GENERATE_MULTIPLE, autoFillDefault: true);

    /// <summary>
    /// Prompt for classifying the source of generation (manual or knowledge base).
    /// </summary>
    public TextBlockProperty PromptGenerateSourceClassifier { get; }
        = new(nameof(PromptGenerateSourceClassifier), "Knowledge Source Classification", PROMPT_CLASSIFY_GENERATE_SOURCE, autoFillDefault: true);

    /// <summary>
    /// Prompt for classifying correlation value between texts.
    /// </summary>
    public TextBlockProperty PromptCorrelationClassifier { get; }
        = new(nameof(PromptCorrelationClassifier), "Correlation Value", PRMOPT_CLASSIFY_CORRELATION, autoFillDefault: true);

    /// <summary>
    /// Prompt text indicating user has a selection.
    /// </summary>
    public StringProperty PromptHasSelection { get; }
        = new(nameof(PromptHasSelection), "Has Selection", PROMPT_HAS_SELECTION, autoFillDefault: true);

    /// <summary>
    /// Prompt text indicating user has no selection.
    /// </summary>
    public StringProperty PromptNoSelection { get; }
        = new(nameof(PromptNoSelection), "No Selection", PROMPT_NO_SELECTION, autoFillDefault: true);

    /// <summary>
    /// Threshold for correlation matching. Values below this are considered no match.
    /// </summary>
    public ValueProperty<float> CorrelationThreshold { get; }
        = new(nameof(CorrelationThreshold), "Correlation Threshold", 0f, "Correlation below this threshold is considered as no match. Set 0 or 1 to disable threshold check.");

    public AIClissifierConfig()
    {
        CorrelationThreshold.Property.WithRange(0, 1);
    }

    /// <summary>
    /// Gets the prompt text for selection state.
    /// </summary>
    /// <param name="hasSelection">Whether the user currently has a selection.</param>
    /// <returns>The appropriate prompt text based on selection state.</returns>
    public string GetPromptSelection(bool hasSelection)
    {
        return hasSelection ? PromptHasSelection.Text : PromptNoSelection.Text;
    }


    /// <summary>
    /// Synchronizes all classifier properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PromptAssistantClassifier.Sync(sync);
        PromptMainClassifier.Sync(sync);
        PromptDocumentClassifier.Sync(sync);
        PromptKnowledgeClassifier.Sync(sync);
        PromptQueryScopeClassifier.Sync(sync);
        PromptGenerateMultipleClassifier.Sync(sync);
        PromptGenerateSourceClassifier.Sync(sync);
        PromptCorrelationClassifier.Sync(sync);

        PromptHasSelection.Sync(sync);
        PromptNoSelection.Sync(sync);

        CorrelationThreshold.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all classifier properties.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        PromptAssistantClassifier.InspectorField(setup);
        PromptMainClassifier.InspectorField(setup);
        PromptDocumentClassifier.InspectorField(setup);
        PromptKnowledgeClassifier.InspectorField(setup);
        PromptQueryScopeClassifier.InspectorField(setup);
        PromptGenerateMultipleClassifier.InspectorField(setup);
        PromptGenerateSourceClassifier.InspectorField(setup);
        PromptCorrelationClassifier.InspectorField(setup);

        PromptHasSelection.InspectorField(setup);
        PromptNoSelection.InspectorField(setup);

        CorrelationThreshold.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for this configuration.
    /// </summary>
    public override string ToString() => L(GetType().ToDisplayText());
}
#endregion

#region AISubdivideConfig

/// <summary>
/// Configuration for AI subdivision operations, including document segmentation and task decomposition.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "AISubdivideConfig", Description = "AI Subdivide Config", Icon = "*CoreIcon|Segment")]
public class AISubdivideConfig : SObjectController
{
    public const string PROMPT_SEGMENT = @"
You are an AI specialized in text segmentation. Your task is to analyze a given document and divide it into multiple meaningful segments, ensuring that each segment is independent and conveys a distinct idea.

Rules:
Detect the sections of the document, similarity between sections, and the importance of each section.
Preserve Logical Structure: Identify natural breaking points based on topics, themes, or logical flow.
Ensure Independence: Each segment should be self-contained, making sense on its own without requiring excessive context from other segments.
Maintain Coherence: Avoid splitting in a way that disrupts readability or meaning.
Adapt to Content Type: For narrative texts, segment based on storyline shifts; for technical documents, split by topics or sections; for discussions, separate distinct arguments or perspectives.
Output Format: Return each segment as a numbered list or structured sections.

IMPORTANT Notice:
  - Ensure the segmentation is clear, meaningful, and improves readability.
";
    
    public const string PROMPT_TASK_SUBDIVIDE = @"
You are an AI expert specializing in task decomposition. 
Your role is to break down complex problems or goals into multiple clear, specific, and actionable sub-tasks. 
Ensure that each sub-task is well-defined, logically sequenced, and contributes effectively to achieving the overall objective. 
Your decomposition should make the process efficient and structured, helping the user accomplish the goal step by step.

IMPORTANT Notice:
 - If the task is too simple or can't be divided, please do not divide it.
 - Ensure that one task can be done by one skill, otherwise please try to divide it.
 ";
    
    public const string PROMPT_BRAINSTORMING = @"
You are an AI expert in brainstorming, dedicated to providing creative support for the user's project or problem. 
Your role is to generate multiple diverse and feasible ideas, offering fresh perspectives and innovative solutions. 
Ensure that your suggestions are varied, practical, and insightful, helping to inspire new directions and assist in decision-making.
";

    /// <summary>
    /// Prompt for document segmentation.
    /// </summary>
    public TextBlockProperty PromptSegment { get; }
        = new(nameof(PromptSegment), "Document Segmentation", PROMPT_SEGMENT, autoFillDefault: true);

    /// <summary>
    /// Prompt for task subdivision/decomposition.
    /// </summary>
    public TextBlockProperty PromptTaskSubdivide { get; }
        = new(nameof(PromptTaskSubdivide), "Task Subdivision", PROMPT_TASK_SUBDIVIDE, autoFillDefault: true);

    /// <summary>
    /// Prompt for brainstorming support.
    /// </summary>
    public TextBlockProperty PromptBrainStorming { get; }
        = new(nameof(PromptBrainStorming), "Brainstorming", PROMPT_BRAINSTORMING, autoFillDefault: true);


    /// <summary>
    /// Synchronizes all subdivide properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PromptSegment.Sync(sync);
        PromptTaskSubdivide.Sync(sync);
        PromptBrainStorming.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all subdivide properties.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        PromptSegment.InspectorField(setup);
        PromptTaskSubdivide.InspectorField(setup);
        PromptBrainStorming.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for this configuration.
    /// </summary>
    public override string ToString() => L(GetType().ToDisplayText());
}

#endregion

#region AIExtractorConfig

/// <summary>
/// Configuration for AI extractor operations, including feature extraction and entity construction.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "AIExtractorConfig", Description = "AI Extractor Config", Icon = "*CoreIcon|Extract")]
public class AIExtractorConfig : SObjectController
{
    public const string PROMPT_FEATURE_EXTRACT = @"
You are an article feature extraction expert. Please understand the text provided by the user and extract multiple entity summaries, including entity name, detailed content, and tags.
1. Entity
Extract all entities mentioned in the article.
An entity should be a noun with a specific meaning and should be the subject in the article.

2. Entity Content
Extract all content related to the entity, should quote the original text completely, preserving complete sentences or paragraphs.
An entity can have multiple content entries, different content should be filled separately.

3. Tags
Extract characteristic tags for this entity, tags must be the entity's type or characteristic adjectives.
Entities should contain as many and rich tags as possible to better classify and retrieve them.

{0}
Output in json, no explanation needed.
";

    public const string PROMPT_ENTITY_CREATE = @"
You are an entity construction expert. Please understand the text provided by the user and construct multiple entities from the text, including entity name, entity type, entity properties, and entity relationships.
The entity name is: {0}

1. Entity Type
Based on the information provided in the context, create a type that accurately matches the description.
Try to create a type that matches the description first, if the new type is similar to an existing type, use the existing type.
The following is a list of entity types for reference:
{2}
If there is no matching type in the list, {3}.

2. Entity Properties
Extract multiple properties from the text, each property contains: property name, description text.
Property names should use common words and be brief, description text should be as detailed and complete as possible.

3. Entity Relationships
Each entity relationship contains: target entity, connecting relationship word.
Entity relationship words should be simple, usually just one word or phrase.

{1}
Output in json, no explanation needed.";
    public const string PROMPT_TYPE_SKIP_NEW = "then skip the extraction of this entity";
    public const string PROMPT_TYPE_CREATE_NEW = "then create a new type";

    /// <summary>
    /// Prompt for feature extraction from text.
    /// </summary>
    public TextBlockProperty PromptFeatureExtract { get; }
        = new(nameof(PromptFeatureExtract), "Feature Extraction", PROMPT_FEATURE_EXTRACT, "{0} represents additional hint, {1} represents type list, {2} represents new type operation", autoFillDefault: true);

    /// <summary>
    /// Prompt for entity construction from text.
    /// </summary>
    public TextBlockProperty PromptEntityCreate { get; }
        = new(nameof(PromptEntityCreate), "Entity Construction", PROMPT_ENTITY_CREATE, autoFillDefault: true);

    /// <summary>
    /// Prompt text for skipping new type extraction.
    /// </summary>
    public StringProperty PromptEntityTypeSkipNew { get; }
        = new(nameof(PromptEntityTypeSkipNew), "Skip New Feature", PROMPT_TYPE_SKIP_NEW, autoFillDefault: true);

    /// <summary>
    /// Prompt text for creating a new entity type.
    /// </summary>
    public StringProperty PromptEntityTypeCreateNew { get; }
        = new(nameof(PromptEntityTypeCreateNew), "Create New Type", PROMPT_TYPE_CREATE_NEW, autoFillDefault: true);

    /// <summary>
    /// Number of times to perform feature extraction on a text.
    /// </summary>
    public ValueProperty<int> FeatureExtractTimes { get; }
        = new(nameof(FeatureExtractTimes), "Feature Extraction Times", 1, "Allow multiple feature extractions on a text to ensure thorough extraction.");


    /// <summary>
    /// Gets the prompt text for new entity type operation.
    /// </summary>
    /// <param name="createNew">Whether to create a new type or skip.</param>
    /// <returns>The appropriate prompt text for the entity type operation.</returns>
    public string GetNewEntityTypeOperation(bool createNew)
    {
        return createNew ? PromptEntityTypeCreateNew.Value : PromptEntityTypeSkipNew.Value;
    }

    /// <summary>
    /// Synchronizes all extractor properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PromptFeatureExtract.Sync(sync);
        PromptEntityCreate.Sync(sync);
        PromptEntityTypeSkipNew.Sync(sync);
        PromptEntityTypeCreateNew.Sync(sync);

        FeatureExtractTimes.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all extractor properties.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        PromptFeatureExtract.InspectorField(setup);
        PromptEntityCreate.InspectorField(setup);
        PromptEntityTypeSkipNew.InspectorField(setup);
        PromptEntityTypeCreateNew.InspectorField(setup);

        FeatureExtractTimes.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for this configuration.
    /// </summary>
    public override string ToString() => L(GetType().ToDisplayText());
}

#endregion

#region AISupportConfig

/// <summary>
/// Configuration for AI support operations, including JSON recovery, requirement design, and question answering.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "AISupportConfig", Description = "AI Support Config", Icon = "*CoreIcon|Support")]
public class AISupportConfig : SObjectController
{
    public const string PROMPT_JSON_RECOVERY = "You are a JSON syntax checking expert. You focus on identifying syntax defects in JSON documents and try to fix these defects, then output the fixed JSON document.";
    public const string PROMPT_REQUIREMENT_DESIGN = @"
You are a requirement analysis expert. Please write a requirement document based on the user's request. It should roughly contain the following information:

1) Name: The name of the theme to design
2) Brief: Design summary
3) HtmlColor: Assigned color
4) FullDocument: Complete theme information, including:
Background introduction, core elements,
{0}

There is no need to design specific values, just give a general direction.
If the user's request information is insufficient, you can supplement more detailed requirements for the user.
Please output plain text, no other encoding symbols.";

    public const string PROMPT_SUMMARY = @"
The user generated a JSON format data structure document through a prompt. Please summarize what content was changed in this operation, just provide a brief report, no need to list json.
";

    public const string PROMPT_CREATE_IDENTIFIER = @"
Please generate a name according to the user's prompt, the name should be an in PascalCase English identifier, and can contain multiple English words, without spaces between words.
";

    public const string PROMPT_ANSWER_QUESTION = "Please answer reasonably based on the materials provided by the user and the questions asked by the user. When answering, strictly follow the information in the user's materials.";


    /// <summary>
    /// Prompt for JSON syntax recovery and fixing.
    /// </summary>
    public TextBlockProperty PromptJsonRecovery { get; }
        = new(nameof(PromptJsonRecovery), "JSON Fix", PROMPT_JSON_RECOVERY, autoFillDefault: true);

    /// <summary>
    /// Prompt for requirement design document generation.
    /// </summary>
    public TextBlockProperty PromptRequirementDesign { get; }
        = new(nameof(PromptRequirementDesign), "Requirement Design", PROMPT_REQUIREMENT_DESIGN, "Write data design requirements, {0} represents the general content to fill in", autoFillDefault: true);

    /// <summary>
    /// Prompt for summarizing JSON data changes.
    /// </summary>
    public TextBlockProperty PromptSummary { get; }
        = new(nameof(PromptSummary), "Summary", PROMPT_SUMMARY, autoFillDefault: true);

    /// <summary>
    /// Prompt for generating PascalCase identifiers.
    /// </summary>
    public TextBlockProperty PromptIdentifier { get; }
        = new(nameof(PromptIdentifier), "Identifier", PROMPT_CREATE_IDENTIFIER, autoFillDefault: true);

    /// <summary>
    /// Prompt for answering questions based on provided materials.
    /// </summary>
    public TextBlockProperty PromptAnswerQuestion { get; }
        = new(nameof(PromptAnswerQuestion), "Answer Question", PROMPT_ANSWER_QUESTION, autoFillDefault: true);

    /// <summary>
    /// Synchronizes all support properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PromptJsonRecovery.Sync(sync);
        PromptRequirementDesign.Sync(sync);
        PromptSummary.Sync(sync);
        PromptIdentifier.Sync(sync);
        PromptAnswerQuestion.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all support properties.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        PromptJsonRecovery.InspectorField(setup);
        PromptRequirementDesign.InspectorField(setup);
        PromptSummary.InspectorField(setup);
        PromptIdentifier.InspectorField(setup);
        PromptAnswerQuestion.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for this configuration.
    /// </summary>
    public override string ToString() => L(GetType().ToDisplayText());
}

#endregion

#region AIKnowledgeConfig

/// <summary>
/// Configuration for AI knowledge base operations, including RAG prompts and synonym handling.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "AIKnowledgeConfig", Description = "AI Knowledge Base Config", Icon = "*CoreIcon|Knowledge")]
public class AIKnowledgeConfig : SObjectController
{
    public const string PROMPT_RAG_KEYWORD = "Please generate 1-3 query keywords based on the user's request query, generate, operation target. Please strictly quote the user's request information, do not create new content. Separate keywords with ','.";
    public const string PROMPT_RAG_OVERVIEW = "Please generate a knowledge base summary of the original content according to the content of the original text and user's requirements. The requirement is pure text format content, not JSON or other format with code symbols, only output summary.";
    public const string PROMPT_RAG_SUMMARY = "Please generate a knowledge base summary of the original content according to the content of the original text and user's requirements. The requirement is pure text format content, not JSON or other format with code symbols, only output summary.";
    public const string PROMPT_RAG_REFERENCE = "The following are some reference materials, please extract effective information from these materials:";

    public const string PROMPT_RAG_FEATURE_ENUMERATE = @"
Please select 1-3 options from the following list that are closest to the concept requested by the user.
{0}

Output the name of the option, do not directly output the user's request.
If there are absolutely no similar options, output '-'. If the user requests all options, output '*'.
Separate with ','. Sort by similarity from high to low. No explanation needed.";
    
    public const string PROMPT_RAG_SYNONYMS_ENUMERATE = @"
Please select the synonym with the highest match degree from the following list: 
{0}

If there is no synonym, output '-'. No explanation needed.";

    public const string PROMPT_RAG_SYNONYMS = @"
Please select 1 synonym from the following list that is a synonym of the word provided by the user:
{0}

The synonym match value should be >= {1} (range is [0~1]).
Output the name of the option, do not directly output the user's request.
If there are absolutely no similar options, output '-', no explanation needed.";


    /// <summary>
    /// Prompt for generating knowledge base query keywords.
    /// </summary>
    public TextBlockProperty PromptRagKeyword { get; }
        = new(nameof(PromptRagKeyword), "Knowledge Base Query Keywords", PROMPT_RAG_KEYWORD, autoFillDefault: true);

    /// <summary>
    /// Prompt for generating knowledge base overview.
    /// </summary>
    public TextBlockProperty PromptRagOverview { get; }
        = new(nameof(PromptRagOverview), "Knowledge Base Overview", PROMPT_RAG_OVERVIEW, autoFillDefault: true);

    /// <summary>
    /// Prompt for generating knowledge base summary.
    /// </summary>
    public TextBlockProperty PromptRagSummary { get; }
        = new(nameof(PromptRagSummary), "Knowledge Base Summary", PROMPT_RAG_SUMMARY, autoFillDefault: true);

    /// <summary>
    /// Prompt for referencing knowledge base materials.
    /// </summary>
    public TextBlockProperty PromptRagReference { get; }
        = new(nameof(PromptRagReference), "Knowledge Base Reference", PROMPT_RAG_REFERENCE, autoFillDefault: true);

    /// <summary>
    /// Prompt for knowledge base feature enumeration.
    /// </summary>
    public TextBlockProperty PromptRagEnumerateFeatures { get; }
        = new(nameof(PromptRagEnumerateFeatures), "Knowledge Base Feature Enumeration", PROMPT_RAG_FEATURE_ENUMERATE, autoFillDefault: true);

    /// <summary>
    /// Prompt for knowledge base synonym selection.
    /// </summary>
    public TextBlockProperty PromptRagSelectSynonyms { get; }
        = new(nameof(PromptRagSelectSynonyms), "Knowledge Base Synonym Selection", PROMPT_RAG_SYNONYMS_ENUMERATE, autoFillDefault: true);

    /// <summary>
    /// Number of top results returned by knowledge base query for single result.
    /// </summary>
    public ValueProperty<int> RagTopK { get; }
        = new(nameof(RagTopK), "Knowledge Base TopK", 5, "Number of TopK returned by knowledge base query");

    /// <summary>
    /// Number of top results returned by knowledge base query for multiple results.
    /// </summary>
    public ValueProperty<int> RagMultipleTopK { get; }
        = new(nameof(RagMultipleTopK), "Knowledge Base Multiple TopK", 10, "Number of multiple TopK returned by knowledge base query");

    /// <summary>
    /// Threshold for synonym matching. Values below this are considered no match.
    /// </summary>
    public ValueProperty<float> SynonymsThreshold { get; }
        = new(nameof(SynonymsThreshold), "Synonym Threshold", 0.8f, "Correlation below this threshold is considered as no match. Set 0 or 1 to disable threshold check.");

    /// <summary>
    /// Gets the TopK value based on whether multiple results are needed.
    /// </summary>
    /// <param name="multiple">Whether to return multiple TopK value.</param>
    /// <returns>The appropriate TopK value.</returns>
    public int GetTopK(bool multiple)
    {
        return multiple ? RagMultipleTopK.Value : RagTopK.Value;
    }

    public AIKnowledgeConfig()
    {
        SynonymsThreshold.Property.WithRange(0, 1);
    }

    /// <summary>
    /// Synchronizes all knowledge base properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PromptRagKeyword.Sync(sync);
        PromptRagOverview.Sync(sync);
        PromptRagSummary.Sync(sync);
        PromptRagReference.Sync(sync);
        PromptRagEnumerateFeatures.Sync(sync);
        PromptRagSelectSynonyms.Sync(sync);

        RagTopK.Sync(sync);
        RagMultipleTopK.Sync(sync);

        SynonymsThreshold.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all knowledge base properties.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        PromptRagKeyword.InspectorField(setup);
        PromptRagOverview.InspectorField(setup);
        PromptRagSummary.InspectorField(setup);
        PromptRagReference.InspectorField(setup);
        PromptRagEnumerateFeatures.InspectorField(setup);
        PromptRagSelectSynonyms.InspectorField(setup);

        RagTopK.InspectorField(setup);
        RagMultipleTopK.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for this configuration.
    /// </summary>
    public override string ToString() => L(GetType().ToDisplayText());
}

#endregion

#region AIDataGenerationConfig

/// <summary>
/// Configuration for AI data generation operations, including JSON creation/update, field filling, and graph generation.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "AIDataGenerationConfig", Description = "AI Data Generation Config", Icon = "*CoreIcon|Generate")]
public class AIDataGenerationConfig : SObjectController
{
    // ---------------- Object
    public const string PROMPT_JSON_CREATE = "Please create a new data object according to the user's requirements.";
    public const string PROMPT_JSON_UPDATE = "Please modify and update the existing data object according to the user's requirements.";

    // ---------------- Field
    public const string PROMPT_FIELD_FILL_SUGGESTION = @"
Please help fill in the {0} field based on the user's requirements and the characteristics of the document.
The description of this field is as follows:
{1}
The type definition of this field is as follows:
{2}

Please infer the direction and suggestions for what {0} needs to fill in based on the context.
Please give a brief description in one sentence.
";

    public const string PROMPT_FIELD_EDIT_SUGGESTION = @"
Please help edit the {0} field based on the user's requirements and the characteristics of the document.
The description of this field is as follows:
{1}
The type definition of this field is as follows:
{2}

Note: This modification targets multiple data, need to judge whether the data provided this time needs modification.
If modification is needed, set IsRelevant to true and give the direction and suggestions for what {0} needs to modify based on the context.
If the provided content does not match the user's modification this time, set IsRelevant to false and give the reason for not modifying.
";

    public const string PROMPT_FIELD_SUGGESTION_ARY = "This field is an array and can have multiple entries.";

    public const string PROMPT_FIELD_SELECT = @"
Please select the closest entry from the following options based on the user's requirements:

{0}

Please only select from the above options, do not get from the user's requirements.
If there is no close entry, then select the roughly matching entry.
Please only return the entry name, no explanation needed.";
    
    public const string PROMPT_FIELD_SELECT_ARY = "This field is an array, please select multiple items and separate them with ','.";

    // ---------------- Graph
    public const string PROMPT_BEHAVIOUR_TREE = @"
Please design a node graph according to the user's requirements. This node graph has the following characteristics:
1. The node graph must be a directed acyclic graph.
2. Must start from a root node.
3. NodeId must conform to naming conventions, cannot be pure numbers or contain special characters.
4. The node type must be one of the following types:
{0}

5. Node type must be filled with full type name.
6. Nodes are connected through fields, the field type is only one type and is suitable for all node types listed above.
7. Fields have two types: Single connectable field and Multiple connectable field. Single can only connect one node, Multiple can connect multiple types.
8. Please make full use of X and Y direction space for layout, mutually connected nodes should be arranged from left to right, if one port connects multiple nodes, should arrange from top to bottom.
9. A node size is approximately 200x100, need to pay attention to the distance between nodes when layout.

Please design a node graph according to the above requirements, including node definition and connection line definition, only output json, no explanation needed.
";

    public const string PROMPT_DATA_FLOW = @"
Please design a node graph according to the user's requirements. This node graph has the following characteristics:
1. The node graph must be a directed acyclic graph.
2. Must start from a root node.
3. NodeId must conform to naming conventions, cannot be pure numbers or contain special characters.
4. The node type must be one of the following types:
{0}

5. Node type must be filled with full type name.
6. Nodes are connected through fields, please select the matching node type based on the field type description.
7. Fields have two types: Single connectable field and Multiple connectable field. Single can only connect one node, Multiple can connect multiple types.
8. Please make full use of X and Y direction space for layout, mutually connected nodes should be arranged from left to right, if one port connects multiple nodes, should arrange from top to bottom.
9. A node size is approximately 200x100, need to pay attention to the distance between nodes when layout.

Please design a node graph according to the above requirements, including node definition and connection line definition, only output json, no explanation needed.
";


    /// <summary>
    /// Prompt for creating a new JSON data object.
    /// </summary>
    public TextBlockProperty PromptJsonCreate { get; }
        = new(nameof(PromptJsonCreate), "Create JSON Object", PROMPT_JSON_CREATE, autoFillDefault: true);

    /// <summary>
    /// Prompt for updating an existing JSON data object.
    /// </summary>
    public TextBlockProperty PromptJsonUpdate { get; }
        = new(nameof(PromptJsonUpdate), "Update JSON Object", PROMPT_JSON_UPDATE, autoFillDefault: true);


    /// <summary>
    /// Prompt for filling a field with suggested content.
    /// </summary>
    public TextBlockProperty PromptFieldFillSuggection { get; }
        = new(nameof(PromptFieldFillSuggection), "Field Fill Suggestion", PROMPT_FIELD_FILL_SUGGESTION, "Infer the general direction suggestion for this field to fill. {0} represents field name, {1} represents field description.", autoFillDefault: true);

    /// <summary>
    /// Prompt for editing/updating a field with suggested changes.
    /// </summary>
    public TextBlockProperty PromptFieldEditSuggection { get; }
        = new(nameof(PromptFieldEditSuggection), "Field Update Suggestion", PROMPT_FIELD_EDIT_SUGGESTION, "Infer the general direction suggestion for this field to update. {0} represents field name, {1} represents field description.", autoFillDefault: true);

    /// <summary>
    /// Prompt hint indicating the field is an array type.
    /// </summary>
    public TextBlockProperty PromptFieldSuggestionArray { get; }
        = new(nameof(PromptFieldSuggestionArray), "Field Fill Suggestion - Array Hint", PROMPT_FIELD_SUGGESTION_ARY, "Add this hint when field is an array.", autoFillDefault: true);

    /// <summary>
    /// Prompt for selecting a single value from a list to fill a field.
    /// </summary>
    public TextBlockProperty PromptFieldSelection { get; }
        = new(nameof(PromptFieldSelection), "Field Fill Selection", PROMPT_FIELD_SELECT, "Select the most matching option from the list to fill the field. {0} represents the selectable list content.", autoFillDefault: true);

    /// <summary>
    /// Prompt hint for selecting multiple values from a list (array field).
    /// </summary>
    public TextBlockProperty PromptFieldSelectionMultiple { get; }
        = new(nameof(PromptFieldSelectionMultiple), "Field Fill Selection - Array Hint", PROMPT_FIELD_SELECT_ARY, "Add this hint when field is an array, need to indicate to separate with ','.", autoFillDefault: true);

    /// <summary>
    /// Prompt for generating a behavior tree node graph.
    /// </summary>
    public TextBlockProperty PromptBehaviorTree { get; }
        = new(nameof(PromptBehaviorTree), "Create Behavior Tree", PROMPT_BEHAVIOUR_TREE, autoFillDefault: true);

    /// <summary>
    /// Prompt for generating a data flow node graph.
    /// </summary>
    public TextBlockProperty PromptDataFlow { get; }
        = new(nameof(PromptDataFlow), "Create Data Flow", PROMPT_DATA_FLOW, autoFillDefault: true);


    /// <summary>
    /// Synchronizes all data generation properties.
    /// </summary>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        PromptJsonCreate.Sync(sync);
        PromptJsonUpdate.Sync(sync);
        
        PromptFieldFillSuggection.Sync(sync);
        PromptFieldEditSuggection.Sync(sync);
        PromptFieldSuggestionArray.Sync(sync);
        
        PromptFieldSelection.Sync(sync);
        PromptFieldSelectionMultiple.Sync(sync);

        PromptBehaviorTree.Sync(sync);
        PromptDataFlow.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for all data generation properties.
    /// </summary>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        PromptJsonCreate.InspectorField(setup);
        PromptJsonUpdate.InspectorField(setup);

        PromptFieldFillSuggection.InspectorField(setup);
        PromptFieldEditSuggection.InspectorField(setup);
        PromptFieldSuggestionArray.InspectorField(setup);

        PromptFieldSelection.InspectorField(setup);
        PromptFieldSelectionMultiple.InspectorField(setup);

        PromptBehaviorTree.InspectorField(setup);
        PromptDataFlow.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for this configuration.
    /// </summary>
    public override string ToString() => L(GetType().ToDisplayText());
}

#endregion

#region LLmModelLevelConfig

/// <summary>
/// Configuration for LLM model level settings, including model type and parameters.
/// </summary>
public class LLmModelLevelConfig : IViewObject
{
    /// <summary>
    /// The type of LLM model to use.
    /// </summary>
    public ValueProperty<LLmModelType> ModelType { get; }
        = new(nameof(ModelType), "Model Type", LLmModelType.Default);

    /// <summary>
    /// The configuration parameters for the LLM model.
    /// </summary>
    public ValueProperty<LLmModelParameter> Config { get; }
        = new(nameof(Config), "Model Config");

    public LLmModelLevelConfig()
    {
        Config.Property.WithOptional();
    }

    /// <summary>
    /// Synchronizes the model type and configuration properties.
    /// </summary>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        ModelType.Sync(sync);
        Config.Sync(sync);
    }

    /// <summary>
    /// Sets up the inspector view for model configuration properties.
    /// </summary>
    public void SetupView(IViewObjectSetup setup)
    {
        ModelType.InspectorField(setup);
        Config.InspectorField(setup);
    }

    /// <summary>
    /// Returns the localized display text for the model type.
    /// </summary>
    public override string ToString()
    {
        return L(ModelType.Value.ToDisplayText());
    }
}
#endregion
