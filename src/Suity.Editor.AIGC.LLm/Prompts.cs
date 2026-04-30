using Suity.Editor.AIGC.Assistants;

namespace Suity.Editor.AIGC;

#region MinimalThinkPrompt

/// <summary>
/// Prompt that instructs the LLM to minimize thinking steps, keeping only brief drafts.
/// </summary>
public class MinimalThinkPrompt : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.MinimalThink";

    /// <inheritdoc/>
    public override string Description => "Minimize thinking";

    /// <inheritdoc/>
    public override string Prompt => @"
## Think step by step, but only keep a minimum draft for each thinking step, with 5 words at most, output in a tag <thought> using following format:
<thought>
thinking 1 ...
thinking 2 ...
thinking 3 ...
...
</thought>
";
}

#endregion

#region UpdateSectionFullPrompt

/// <summary>
/// Prompt that updates section content fully based on new information.
/// </summary>
public class UpdateSectionFullPrompt : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.UpdateSection.Full";

    /// <inheritdoc/>
    public override string Description => "Update Section (Full)";

    /// <inheritdoc/>
    public override string Prompt => @"
# Please update the origin content according to the new information.

# Only output updated sections, do NOT output the unchanged sections.

# If there are nothing to update, output 'No changes'.

# Origin content:
";
}

#endregion

#region UpdateSectionPartialPrompt

/// <summary>
/// Prompt that updates only the changed sections based on new information.
/// </summary>
public class UpdateSectionPartialPrompt : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.UpdateSection.Partial";

    /// <inheritdoc/>
    public override string Description => "Update Section (Partial)";

    /// <inheritdoc/>
    public override string Prompt => @"
# Please update the origin content according to the new information and output the changed sections.
- Output the newly added and updated sections only.
- If there are nothing to update, output 'No changes'.

## First output the section names that need to be updated, as following format:
<update>
Section 1
Section 2
...
</update>

## Then output the newly added and updated sections with the same format in origin content.

## Origin content:
";
}

#endregion

#region Model Shared

/// <summary>
/// Prompt that defines the data modeling format for structure definitions.
/// </summary>
public class Model_Format : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Model.Format";

    /// <inheritdoc/>
    public override string Description => "Data modeling format";

    /// <inheritdoc/>
    public override string Prompt => @"
Output structure definition in a tag <type>.

1. Common enum:
<type name='EnumName' def='Enum' doc='enum description'>
- Value1 # value doc
- Value2 # value doc
...
</type>

2. Common struct:
<type name='StructName' def='Struct' doc='struct description'>
- Field1:Type1 # field doc
- Field2:Type2 # field doc
...
</type>

3. Abstract-derived struct:
3.1 - Abstract structure format:
<type name='AbstractStructName' def='Abstract' doc='struct description'>
...
</type>

3.2 - Derived structure format:
<type name='DerivedStructName' def='Struct' base='AbstractStructName' doc='struct description'>
...
</type>
** Only 'Abstract' type can be extended, do NOT extends 'Struct' type **

4. Nullable design pattern for structs that has specific optional fields:
<type name='MainStruct' def='Struct'>
  ...
  NullableFieldA: NullableStructA [Nullable]
  NullableFieldB: NullableStructB [Nullable]
  ...
</type>

Always nullable struct is defined as:
<type name='NullableStructA' def='Struct' usage='Nullable'>
...
</type>

5. Others:
- Array: Add '[]' suffix to the field type to indicate that the field is an array, such as: 'Field1:Type[]'.
- Range: Replace '[0..x]' or '[min..max]' marker with a actual min-max value range for the numeric fields.
";


    /*
    4. Data Usage:
    - Add 'usage' attribute to the <type> tag to indicate the data usage of the structure, such as:
    <type name='SomeStruct' def='Struct' usage='...'>
    ...
    </type>

    Options in 'usage' attribute:
    - DataGrid: used to design data grid / data table.
    - FlowGraph: used to design flow graph.
    - TreeGraph: used to design tree graph.
    - Config: used for configuration.
    - EntityData: used to store entity data in storage.
    - Action: used to define action.
    - Nullable: this type can be nullable.
     */

    /*    6. Value range decorator:
        - Add '[min..max]' decorator to the numeric fields to indicate the value range, such as: 'Field1:int # doc [0..100]'.
    */

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Prompt that defines the data modeling format for game action structures.
/// </summary>
public class GameAction_Format : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.GameAction.Format";

    /// <inheritdoc/>
    public override string Description => "Data modeling format";

    /// <inheritdoc/>
    public override string Prompt => @"
Output structure definition in a tag <type>.

1. Common enum:
<type name='EnumName' def='Enum' doc='enum description'>
- Value1 # doc
- Value2 # doc
...
</type>

2. Common struct:
<type name='StructName' def='Struct' doc='struct description'>
- Field1:Type1 # doc
- Field2:Type2 # doc
...
</type>

3. Entity action struct:
<type name='DerivedAction' def='Struct' base='Suity.Gaming.GameAction' usage='Action' doc='struct description'>
- OtherField1: Type1 # doc
- OtherField2: Type2 # doc
...
</type>

4. Others:
- Array: Add '[]' suffix to the field type to indicate that the field is an array, such as: 'Field1:Type[]'.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Prompt that defines the field type guidelines for data modeling.
/// </summary>
public class Model_FieldType : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Model.FieldType";

    /// <inheritdoc/>
    public override string Description => "Data modeling field type";

    /// <inheritdoc/>
    public override string Prompt => @"
- Use native type instead of special type if possible : int, float, string, bool, DateTime, etc.
- Use string for Color field.
- Use string for Resource location.
- Use Int2, IntSize, IntRange, FloatRange for position, size and range.
";

    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

#endregion

#region Article

/// <summary>
/// Prompt for generating articles based on title, user requirements, and parent content.
/// </summary>
public class Common_ArticleGenerate : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Article.Generate";

    /// <inheritdoc/>
    public override string Description => "Generate Article";

    /// <inheritdoc/>
    public override string Prompt => @"
You are a professional article writer. Please generate an article according to the following information.

# Article title:
{{TITLE}}

# User requirement:
{{PROMPT}}

# Parent article content:
{{PARENT}}

Now please generate the article content.
The speech language is '{{SPEECH_LANGUAGE}}'. 
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Prompt for optimizing existing article content based on user requirements.
/// </summary>
public class Common_ArticleOptimize : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Article.Optimize";

    /// <inheritdoc/>
    public override string Description => "Optimize Article";

    /// <inheritdoc/>
    public override string Prompt => @"
You are a professional article optimizer. Please optimize the following article according to the following information.

# Article content:
{{CONTENT}}

# User requirement:
{{PROMPT}}

# Parent article content:
{{PARENT}}

Now please generate the article content.
The speech language is '{{SPEECH_LANGUAGE}}'. 
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Prompt for summarizing article content.
/// </summary>
public class Common_ArticleSummarize : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Article.Summarize";

    /// <inheritdoc/>
    public override string Description => "Summarize Article";

    /// <inheritdoc/>
    public override string Prompt => @"
You are a professional article summarizer. Please summarize the following article according to the following information.

# Main article content:
{{CONTENT}}

# Parent article reference:
{{PARENT}}

Now please generate the article summary.
The speech language is '{{SPEECH_LANGUAGE}}'. 
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Prompt for subdividing articles into multiple sub-topic titles.
/// </summary>
public class Common_ArticleSubdivide : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Article.Subdivide";

    /// <inheritdoc/>
    public override string Description => "Subdivide Article";

    /// <inheritdoc/>
    public override string Prompt => @"
You are a professional article subdivider. Please create multiple sub-topic title according to the following information:

# Main article content:
{{CONTENT}}

# User requirement:
{{PROMPT}}

# Parent article reference:
{{PARENT}}

# Now please generate the sub-topic title list with the following format:
<topic title='Title1'>Overview in one sentence.</topic>
<topic title='Title2'>Overview in one sentence.</topic>
<topic title='Title3'>Overview in one sentence.</topic>
...

The speech language is '{{SPEECH_LANGUAGE}}'. 
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Prompt for segmenting articles into multiple sections with titles.
/// </summary>
public class Common_ArticleSegment : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Article.Segment";

    /// <inheritdoc/>
    public override string Description => "Segment Article";

    /// <inheritdoc/>
    public override string Prompt => @"
You are a professional article segmenter. Please segment the following article into multiple segments:

# Article content:
{{CONTENT}}

# Now please generate the sub-topic title list with the following format:
<segment title='Title1'>Segment content</segment>
<segment title='Title2'>Segment content</segment>
<segment title='Title3'>Segment content</segment>
...

# At the end, create a summary segment with the following format:
<summary>Summary content</summary>

Notice:
- Keep the segment content same as the original article.
- The speech language is '{{SPEECH_LANGUAGE}}'. 
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Prompt for answering questions based on article content.
/// </summary>
public class Common_ArticleAnswerQuestion : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Common.Article.AnswerQuestion";

    /// <inheritdoc/>
    public override string Description => "Answer question";

    /// <inheritdoc/>
    public override string Prompt => @"
You are a professional article reader. You have been hired to answer the following question.

# Here is the original article:
{{CONTENT}}

# Parent article reference:
{{PARENT}}

# Here is the question:
{{PROMPT}}

Please answer the question according to the article, and output the result in plain text format.

# Important Notice:
- Output speech language is: {{SPEECH_LANGUAGE}}.
";
}

#endregion