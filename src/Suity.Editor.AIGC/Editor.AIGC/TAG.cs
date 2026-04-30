namespace Suity.Editor.AIGC;

public static class TAG
{
    #region Common

    /// <summary>
    /// User Input
    /// </summary>
    public const string PROMPT = "{{PROMPT}}";

    /// <summary>
    /// Language
    /// </summary>
    public const string SPEECH_LANGUAGE = "{{SPEECH_LANGUAGE}}";

    /// <summary>
    /// Output Format
    /// </summary>
    public const string OUTPUT_FORMAT = "{{OUTPUT_FORMAT}}";

    /// <summary>
    /// Count
    /// </summary>
    public const string COUNT = "{{COUNT}}";

    /// <summary>
    /// Plan
    /// </summary>
    public const string PLAN = "{{PLAN}}";

    /// <summary>
    /// Topic
    /// </summary>
    public const string TOPIC = "{{TOPIC}}";

    public const string SUBJECT = "{{SUBJECT}}";

    /// <summary>
    /// Template
    /// </summary>
    public const string TEMPLATE = "{{TEMPLATE}}";

    /// <summary>
    /// Thinking
    /// </summary>
    public const string THINK = "{{THINK}}";

    /// <summary>
    /// Parent
    /// </summary>
    public const string PARENT = "{{PARENT}}";

    /// <summary>
    /// Origin
    /// </summary>
    public const string ORIGIN = "{{ORIGIN}}";

    /// <summary>
    /// Document
    /// </summary>
    public const string DOCUMENT = "{{DOCUMENT}}";

    /// <summary>
    /// Name
    /// </summary>
    public const string NAME = "{{NAME}}";

    /// <summary>
    /// Naming
    /// </summary>
    public const string NAMING = "{{NAMING}}";

    /// <summary>
    /// Selection
    /// </summary>
    public const string SELECTION = "{{SELECTION}}";

    /// <summary>
    /// Nullable
    /// </summary>
    public const string NULLABLE = "{{NULLABLE}}";

    /// <summary>
    /// Knowledge
    /// </summary>
    public const string KNOWLEDGE = "{{KNOWLEDGE}}";

    /// <summary>
    /// Rule
    /// </summary>
    public const string RULE = "{{RULE}}";

    /// <summary>
    /// Header
    /// </summary>
    public const string HEADER = "{{HEADER}}";

    /// <summary>
    /// Reference
    /// </summary>
    public const string REFERENCE = "{{REFERENCE}}";

    /// <summary>
    /// Reference
    /// </summary>
    public const string TITLE = "{{TITLE}}";

    /// <summary>
    /// Reference
    /// </summary>
    public const string CONTENT = "{{CONTENT}}";


    public const string TARGET = "{{TARGET}}";

    public const string PRESET = "{{PRESET}}";

    #endregion

    #region List
    /// <summary>
    /// Section List
    /// </summary>
    public const string SECTION_LIST = "{{SECTION_LIST}}";


    /// <summary>
    /// Case Study List
    /// </summary>
    public const string CASE_STUDY_LIST = "{{CASE_STUDY_LIST}}";

    /// <summary>
    /// Expert List
    /// <summary>
    public const string EXPERT_LIST = "{{EXPERT_LIST}}";

    /// <summary>
    /// Type List
    /// </summary>
    public const string TYPE_LIST = "{{TYPE_LIST}}";

    /// <summary>
    /// Data List
    /// </summary>
    public const string DATA_LIST = "{{DATA_LIST}}";

    /// <summary>
    /// Full List
    /// </summary>
    public const string FULL_LIST = "{{FULL_LIST}}";

    public const string ENTITY_LIST = "{{ENTITY_LIST}}";

    public const string FACTOR_LIST = "{{FACTOR_LIST}}";

    /// <summary>
    /// Selected List
    /// </summary>
    public const string SELECTED_LIST = "{{SELECTED_LIST}}";

    /// <summary>
    /// Previous List
    /// </summary>
    public const string PREVIOUS_LIST = "{{PREVIOUS_LIST}}";

    #endregion

    #region Field

    /// <summary>
    /// Field Name
    /// </summary>
    public const string FIELD_NAME = "{{FIELD_NAME}}";

    /// <summary>
    /// Field Description
    /// </summary>
    public const string FIELD_DESC = "{{FIELD_DESC}}";

    /// <summary>
    /// Field Type Description
    /// </summary>
    public const string FIELD_TYPE_DESC = "{{FIELD_TYPE_DESC}}";
    #endregion

    #region Overview
    /// <summary>
    /// Product ID
    /// </summary>
    public const string PRODUCT_ID = "{{PRODUCT_ID}}";

    /// <summary>
    /// Product Name
    /// </summary>
    public const string PRODUCT_NAME = "{{PRODUCT_NAME}}";

    /// <summary>
    /// Requirement Document
    /// </summary>
    public const string REQUIREMENT_DOCUMENT = "{{REQUIREMENT_DOCUMENT}}";

    /// <summary>
    /// Design Overview
    /// </summary>
    public const string OVERVIEW_DESIGN = "{{OVERVIEW_DESIGN}}";

    /// <summary>
    /// Narrative Overview
    /// </summary>
    public const string OVERVIEW_NARRATIVE = "{{OVERVIEW_NARRATIVE}}";

    /// <summary>
    /// All Expert Overview
    /// </summary>
    public const string OVERVIEW_ALL_EXPERT = "{{OVERVIEW_ALL_EXPERT}}";
    #endregion

    #region Expert
    public const string EXPERT = "{{EXPERT}}";

    /// <summary>
    /// Expert Tooltips
    /// </summary>
    public const string EXPERT_TOOLTIPS = "{{EXPERT_TOOLTIPS}}";

    /// <summary>
    /// Expert Design
    /// </summary>
    public const string EXPERT_DESIGN = "{{EXPERT_DESIGN}}";

    /// <summary>
    /// Expert Narrative
    /// </summary>
    public const string EXPERT_NARRATIVE = "{{EXPERT_NARRATIVE}}";

    /// <summary>
    /// Expert Narrative
    /// </summary>
    public const string EXPERT_GUIDING = "{{EXPERT_GUIDING}}";

    #endregion

    #region Guiding
    /// <summary>
    /// Guiding
    /// </summary>
    public const string GUIDING = "{{GUIDING}}";

    /// <summary>
    /// Suggestions
    /// </summary>
    public const string SUGGESTIONS = "{{SUGGESTIONS}}";
    #endregion

    #region Model
    /// <summary>
    /// Data Model Plan
    /// </summary>
    public const string DATA_MODEL_PLAN = "{{DATA_MODEL_PLAN}}";

    /// <summary>
    /// Data Model
    /// </summary>
    public const string DATA_MODEL = "{{DATA_MODEL}}";

    /// <summary>
    /// Field Type Tips
    /// </summary>
    public const string FIELD_TYPE_TIPS = "{{FIELD_TYPE_TIPS}}";


    public const string MAIN_MODEL = "{{MAIN_MODEL}}";

    public const string ENTITY_MODEL = "{{ENTITY_MODEL}}";

    public const string ACTION_MODEL = "{{ACTION_MODEL}}";
    #endregion

    #region Data
    /// <summary>
    /// Data Table
    /// </summary>
    public const string DATA_TABLE = "{{DATA_TABLE}}";

    /// <summary>
    /// Max Data Count
    /// </summary>
    public const string MAX_DATA_COUNT = "{{MAX_DATA_COUNT}}";

    /// <summary>
    /// Table Type
    /// </summary>
    public const string TABLE_TYPE = "{{TABLE_TYPE}}";

    /// <summary>
    /// Type Description
    /// </summary>
    public const string TYPE_SPEC = "{{TYPE_SPEC}}";

    /// <summary>
    /// Category Name
    /// </summary>
    public const string CATEGORY_NAME = "{{CATEGORY_NAME}}";

    /// <summary>
    /// Category Description
    /// </summary>
    public const string CATEGORY_DESCRIPTION = "{{CATEGORY_DESCRIPTION}}";

    /// <summary>
    /// Data Name
    /// </summary>
    public const string DATA_NAME = "{{DATA_NAME}}";

    /// <summary>
    /// Data Localized Name
    /// </summary>
    public const string LOCAL_NAME = "{{LOCAL_NAME}}";

    /// <summary>
    /// Data Description
    /// </summary>
    public const string DATA_DESCRIPTION = "{{DATA_DESCRIPTION}}";

    /// <summary>
    /// Data Guiding
    /// </summary>
    public const string DATA_GUIDING = "{{DATA_GUIDING}}";

    /// <summary>
    /// Table Plan
    /// </summary>
    public const string TABLE_PLAN = "{{TABLE_PLAN}}";

    /// <summary>
    /// Schema
    /// </summary>
    public const string SCHEMA = "{{SCHEMA}}";

    /// <summary>
    /// Classification
    /// </summary>
    public const string CLASSIFY = "{{CLASSIFY}}";

    public const string ARRAY = "{{ARRAY}}";

    public const string OPTIONAL = "{{OPTIONAL}}";

    #endregion

    #region Node

    /// <summary>
    /// Connecting Rule
    /// </summary>
    public const string CONNECTING_RULE = "{{CONNECTING_RULE}}";

    #endregion

    #region Fix
    /// <summary>
    /// Fixture
    /// </summary>
    public const string FIXTURE = "{{FIXTURE}}";

    /// <summary>
    /// Problem
    /// </summary>
    public const string PROBLEM = "{{PROBLEM}}";

    /// <summary>
    /// Correct
    /// </summary>
    public const string CORRECT = "{{CORRECT}}";

    /// <summary>
    /// Last
    /// </summary>
    public const string LAST = "{{LAST}}";
    #endregion

    #region Code

    public const string SOURCE_CODE = "{{SOURCE_CODE}}";

    public const string CODING_LANGUAGE = "{{CODING_LANGUAGE}}";

    public const string BASE_FRAMEWORK = "{{BASE_FRAMEWORK}}";

    public const string CODING_TAG_BEGIN = "{{CODING_TAG_BEGIN}}";
    public const string CODING_TAG_END = "{{CODING_TAG_END}}";

    public const string ORIGIN_CODE = "{{ORIGIN_CODE}}";
    public const string MODIFIED_CODE = "{{MODIFIED_CODE}}";

    #endregion
}