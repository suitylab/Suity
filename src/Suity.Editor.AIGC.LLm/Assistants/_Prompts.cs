namespace Suity.Editor.AIGC.Assistants;

#region Data DataGrid

/// <summary>
/// AI prompt for generating data table designs in a data grid format.
/// </summary>
public class Workflow_Data_DataGrid : CoreAIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.Data.DataGrid";

    /// <inheritdoc/>
    public override string Description => "Data table";

    /// <inheritdoc/>
    public override string Prompt => @"
You are the creative data design expert for '{{EXPERT}}'.
You are asked to design data items for '{{TABLE_TYPE}}'.
You are given some materials for this task.

# Narrative writing for this expert:
{{EXPERT_NARRATIVE}}

# Current category : {{CATEGORY_NAME}}.

# Data structure schema for '{{TABLE_TYPE}}':
```json
{{SCHEMA}}
```

# Now Your task is to design '{{DATA_NAME}}'.
LocalName: {{LOCAL_NAME}}
## Data item description:
{{DATA_DESCRIPTION}}.

## Data creation guiding:
{{DATA_GUIDING}}

## Write a detailed design for this data item.

## Before output the categories, think about the following:
- List key points for this item : '{{DATA_NAME}}'.
- Output tht thinking in a <thought> tag.

## Field type tips:
{{FIELD_TYPE_TIPS}}

## Output format:
<data name='Data name' local='Localized Name' color='html color (#000000)'>
detailed data design in markdown format...
</data>
...

# Important Notice:
- Output content in article and paragraph in markdown format in <data> tag.
- Do NOT output any json format, use plain text instead.
- Do NOT output any numeric value, use adjective and description instead.
- The speech language of tag inner text should be '{{SPEECH_LANGUAGE}}'. 
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.CreativeWriting;
}

#endregion

#region Data FlowGraph

/// <summary>
/// AI prompt for generating data flow chart designs based on node graph descriptions.
/// </summary>
public class Workflow_Data_FlowGraph : CoreAIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.Data.FlowGraph";

    /// <inheritdoc/>
    public override string Description => "Data flow chart";

    /// <inheritdoc/>
    public override string Prompt => @"
# Now your task is to create a node graph based on the following description:
{{PROMPT}}

# Node graph type:
{{TABLE_TYPE}}

# Current category: {{CATEGORY_NAME}}.

# Use guiding:
{{GUIDING}}

# References:
## Design document for this section:
{{EXPERT_DESIGN}}

## Narrative writing for this section:
{{EXPERT_NARRATIVE}}

# Rules for this node graph document:
{{RULE}}

## Node graph creation guiding:
{{DATA_GUIDING}}

# Before output the node graph, think about the following and output to a <thought> tag:
<thought>
- The theme of the node graph.
- How many nodes that is suitable for this node graph?
- Is there any multiple branches in this node graph, or is it a single branch?
</thought>

# Please output a creation plan for the node graph, including the following sections:
1. Decription of the node graph.
2. Overview of each nodes, including:
  - Node id, in PascalCase.
  - Node type, listed in Rules above.
  - Description and overview of the node.
3. Connection between nodes using mermaid format.

# Notice:
- The node graph must be a directed acyclic graph (DAG).
- Output document with markdown format.
- The speech language of tag inner text should be '{{SPEECH_LANGUAGE}}'. 
";
    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.CreativeWriting;
}

#endregion

#region Data TreeGraph

/// <summary>
/// AI prompt for generating data tree chart designs with hierarchical node structures.
/// </summary>
public class Workflow_Data_TreeGraph : CoreAIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.Data.TreeGraph";

    /// <inheritdoc/>
    public override string Description => "Data tree chart";

    /// <inheritdoc/>
    public override string Prompt => @"
You are the data planner for creating a node graph data.
# The description of the node graph is as follow:
{{PROMPT}}

# Node graph type:
{{TABLE_TYPE}}

# The name of root node in node graph is : '{{DATA_NAME}}'.
LocalName: {{LOCAL_NAME}}
'
# Current category : {{CATEGORY_NAME}}.

# Design guiding:
{{GUIDING}}

# References:
## Design document for this section:
{{EXPERT_DESIGN}}

## Narrative writing for this section:
{{EXPERT_NARRATIVE}}

# Rules for this node graph document:
{{RULE}}

## Node graph creation guiding:
{{DATA_GUIDING}}

# Before output the node graph, think about the following and output to a <thought> tag:
1. The theme of the node graph.
2. How many nodes that is suitable for this node graph?
3. Is there any multiple branches in this node graph, or is it a single branch?

# Please output a creation plan for the node graph, including the following sections:
1. Decription of the node graph.
2. Overview of each nodes, including:
  - Node id, in PascalCase.
  - Node type, listed in Rules above.
  - Description and overview of the node.
  ** The first node should be : '{{DATA_NAME}}' **
3. Connection between nodes using mermaid format.

# Notice:
- The node graph must be a directed acyclic graph (DAG).
- Output document with markdown format.
- The first node should be : '{{DATA_NAME}}'
- The speech language of tag inner text should be '{{SPEECH_LANGUAGE}}'.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.CreativeWriting;
}


#endregion

#region Core_ComplexField_Suggestion_Create
/// <summary>
/// AI prompt for extracting and suggesting content when creating a new complex field.
/// </summary>
public class Core_ComplexField_Suggestion_Create : CoreAIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.Suggestion.Create";

    /// <inheritdoc/>
    public override string Description => "Complex Field Suggestion - Create";

    /// <inheritdoc/>
    public override string Prompt => @"
You are a data planner for creating data field: '{{FIELD_NAME}}'.

# The parent data is as follow:
```json
{{PARENT}}
```

# The parent data structure is as follow:
```json
{{TYPE_SPEC}}
```

# Now focus on the field: '{{FIELD_NAME}}'. Field description:
{{FIELD_DESC}}

# The '{{FIELD_NAME}}' field data schema is as follows:
```json
{{FIELD_TYPE_DESC}}
```

# Now your task is to extract the key information for field: '{{FIELD_NAME}}' from the user's guiding below.

# The following information is the detail data design guiding from user:
{{GUIDING}}

# First output the thoughts as follow:
<thought>
- Understanding the meaning of the field: '{{FIELD_NAME}}'.
- Is it means empty for this field: '{{FIELD_NAME}}' in origin guiding and why?
- How many item should be in this field: '{{FIELD_NAME}}' if it is an array?
- Is there sufficient information for this field: '{{FIELD_NAME}}' in origin guiding?, if not, what is the missing information?
- Detect the format of orginal guiding. (plain text, markdown, json, xml, etc.)
</thought>

## Guiding extraction rules:
### When the guiding from user has sufficient information for the field: '{{FIELD_NAME}}':
- Extract that partial information from the guiding and do NOT modify it.
- The format of extracted content should follow the user's guiding.

### When the guiding from user is insufficient for the field: '{{FIELD_NAME}}':
- Extract the information from the guiding that can be used to fill in the field: '{{FIELD_NAME}}'.
- Fill in the missing information with your own knowledge and experience.
- The format of newly added content should follow the user's guiding.

### output 'NULL' if the origin guiding has 'none' meaning for field: '{{FIELD_NAME}}'.

## Output format:
<data>
Detailed data design plan...
</data>
...

{{ARRAY}}

{{OPTIONAL}}

# Notice:
- Keep the origin guiding as much as possible unless output 'NULL'.
- The output article format is based on the original user's guiding.
- Do NOT output any numeric value, use adjective and description instead.
- The speech language is based on origin user's guiding.
";

    // - The speech language is: {{SPEECH_LANGUAGE}}.

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.CreativeToolCalling;
}
#endregion

#region Core_ComplexField_Suggestion_Guiding
/// <summary>
/// AI prompt for generating design guidance when filling in a complex field.
/// </summary>
public class Core_ComplexField_Suggestion_Guiding : CoreAIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.Suggestion.Guiding";

    /// <inheritdoc/>
    public override string Description => "Complex Field Suggestion - Guide";

    /// <inheritdoc/>
    public override string Prompt => @"
Your task is to fill in the '{{FIELD_NAME}}' field of parent data, based on field definition and Detail data design guiding.

# The parent data is as follow:
```json
{{PARENT}}
```

# The '{{FIELD_NAME}}' field description:
{{FIELD_DESC}}

# The '{{FIELD_NAME}}' field content schema is as follows:
```json
{{FIELD_TYPE_DESC}}
```

# Detail data design guiding:
{{GUIDING}}

# Notice:
- Edit type is: Create.
- Name should be in English PascalCase.
- Please infer the content direction and suggestions that need to be filled in '{{FIELD_NAME}}' based on the context.
- Please provide a design guiding for the creation of the field '{{FIELD_NAME}}' in paragraph.
- Output speech language is: {{SPEECH_LANGUAGE}}.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.CreativeToolCalling;
}
#endregion

#region Core_ComplexField_Suggestion_Modify
/// <summary>
/// AI prompt for suggesting modifications to an existing complex field.
/// </summary>
public class Core_ComplexField_Suggestion_Modify : CoreAIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.Suggestion.Modify";

    /// <inheritdoc/>
    public override string Description => "Complex Field Suggestion - Modify";

    /// <inheritdoc/>
    public override string Prompt => @"
Your task is to edit the '{{FIELD_NAME}}' field of parent data, based on field definition and Detail data design guiding.

# The parent data is as follow:
```json
{{PARENT}}
```

# The '{{FIELD_NAME}}' field description:
{{FIELD_DESC}}

# The '{{FIELD_NAME}}' field definition is as follows:
```json
{{FIELD_TYPE_DESC}}
```

# the original json that needs to be modified:
```json
{{ORIGIN}}
```

# Detail data design guiding:
{{GUIDING}}

# Notice:
- Please infer the content direction and suggestions that need to be filled in '{{FIELD_NAME}}' based on the context.
- Please provide a brief description in 1-3 sentences.
- Output speech language is: {{SPEECH_LANGUAGE}}.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.CreativeToolCalling;
}
#endregion

#region Core_ComplexField_SelectStruct

/// <summary>
/// AI prompt for selecting a relevant structure from available options based on user requirements.
/// </summary>
public class Core_ComplexField_SelectStruct : CoreAIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.SelectStruct";

    /// <inheritdoc/>
    public override string Description => "Complex Field Selection Structure";

    /// <inheritdoc/>
    public override string Prompt => @"
Please select the relavent structure name from the following options according to the user's requirements:

{{SELECTION}}

Please only select from the above entries and do not obtain from user requests.
If there are no similar entries, you can choose entries that roughly match.
Please only return the structure name without any explanation.
{{NULLABLE}}
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.Selection;
}

#endregion

#region Core_ComplexField_LinkedDataSelector

/// <summary>
/// AI prompt for selecting a single data item from a data list.
/// </summary>
public class Core_ComplexField_LinkedDataSelector_Single : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.LinkedDataSelector.Single";

    /// <inheritdoc/>
    public override string Description => "Single Data Selector";

    /// <inheritdoc/>
    public override string Prompt => @"
You are the data expert that helps to select data items from Data List.

# Data List:
{{DATA_LIST}}

# Last selected data list:
{{SELECTED_LIST}}

# Please select 1 data name from Data List according to the following requirement:
{{GUIDING}}

# Important Notice:
- The output names should be a standard identifier name in English.
- Do NOT select name in requirement, select name in Data List.
- Select name attribute in <data name='..'> tag.
- Select 1 data name only.
- If names in Data List don't match the requirement, select the most relevant ones.
- If there is no relevant data, output 'null'.
- If you can't find enough data items, output as many as you can.
- Do NOT create new name.
- Export the names only, do NOT output any other information.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.Selection;
}

/// <summary>
/// AI prompt for repairing a single data selection when selected names are not found in the data list.
/// </summary>
public class Core_ComplexField_LinkedDataSelector_Single_Fix : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.LinkedDataSelector.Single.Fix";

    /// <inheritdoc/>
    public override string Description => "Single Data Selector - Repair";

    /// <inheritdoc/>
    public override string Prompt => @"
There are some problems with the selection from last generation.
The following name are not found in Data List:
{{PROBLEM}}

Please remove the missing names from selection, reselect the correct names.
Export the names only, do NOT output any other information.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.Selection;
}


/// <summary>
/// AI prompt for selecting multiple data items from a data list.
/// </summary>
public class Core_ComplexField_LinkedDataSelector_Multiple : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.LinkedDataSelector.Multiple";

    /// <inheritdoc/>
    public override string Description => "Multiple Data Selectors";

    /// <inheritdoc/>
    public override string Prompt => @"
You are the data expert that helps to select data items from Data List.

# Data List:
{{DATA_LIST}}

# Last selected data list:
{{SELECTED_LIST}}

# Please select data items from Data List according to the following requirement:
{{GUIDING}}

# Important Notice:
- The output names should be a standard identifier name in English.
- Do NOT select name in requirement, select name in Data List.
- Select name attribute in <data name='..'> tag.
- Select {{COUNT}} items, separated by comma.
- If names in Data List don't match the requirement, select the most relevant ones.
- If there is no relevant data, output 'null'.
- If you can't find enough data items, output as many as you can.
- Export the names only, do NOT output any other information.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.Selection;
}

/// <summary>
/// AI prompt for repairing a multiple data selection when selected names are not found in the data list.
/// </summary>
public class Core_ComplexField_LinkedDataSelector_Multiple_Fix : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.ComplexField.LinkedDataSelector.Multiple.Fix";

    /// <inheritdoc/>
    public override string Description => "Multiple Data Selectors - Repair";

    /// <inheritdoc/>
    public override string Prompt => @"
There are some problems with the selection from last generation.
The following names are not found in Data List:
{{PROBLEM}}

The Correct names are:
{{CORRECT}}

Please remove the missing names from selection, reselect the correct names.
Export the names only, do NOT output any other information.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.Selection;
}
#endregion

#region Core_Assistant_SelectAssistant

/// <summary>
/// AI prompt for selecting the most appropriate assistant based on user needs.
/// </summary>
public class Core_Assistant_SelectAssistant : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.Assistant.SelectAssistant";
    /// <inheritdoc/>
    public override string Description => "Selection Assistant";
    /// <inheritdoc/>
    public override string Prompt => @"
You are an AI assistant that helps user to select an assistant based on the user's needs. 
{{FULL_LIST}}

<Assistant>Not-Found</Assistant>
User request is not recognized.

# Selection Guide:
- Please select one of the templates based on the user's needs. 
- If the user's request is ambiguous, please select the template that is most likely to meet the user's needs.
- If the user's request is not in the list, please select the Not-Found template.
- If the user's request is not recognized, please select the Not-Found template.

# IMPORTANT Notice:
- Export the full name in the tag <Assistant>, and nothing else.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.Selection;
}
#endregion

#region Core_Assistant_SelectTool

/// <summary>
/// AI prompt for selecting the most appropriate AI tool based on user requests.
/// </summary>
public class Core_Assistant_SelectTool : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Core.Assistant.SelectTool";
    /// <inheritdoc/>
    public override string Description => "Selection Tool";
    /// <inheritdoc/>
    public override string Prompt => @"
You are an intelligent agent that parses user's request and selects one appropriate AI tool to use.
You will be given the tool call information in the tool call schema.
Please select one of the tools above based on the user's needs, and provide the following information:

1. The full name of the tool to call.
2. The tool parameters to pass to the tool.

# Selection Guiding:
- If the user's request is not in the list, please select the 'ToolNotFound'.
- If the user's request is not recognized, please select the 'ToolNotFound'.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.Selection;
}


#endregion