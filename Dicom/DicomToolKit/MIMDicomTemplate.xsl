<?xml version="1.0" encoding="iso-8859-1"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:fn="http://www.w3.org/2005/xpath-functions" 
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:user="urn:local">

  <msxsl:script language="C#" implements-prefix="user">
    <![CDATA[
     public string strip(string name)
     {
        return name.Replace("_", "");
     }
     
     public string indent(int level)
     {
        string result = "";
        for(int n = 0; n < level; n++)
        {
           result += "\t";
        }
        return result;
     }  
     ]]>
  </msxsl:script>

  <xsl:template match="/">
    <xsl:element name ="dicom">
      <xsl:text>&#10;</xsl:text>
      <xsl:apply-templates select="*" />
    </xsl:element>
  </xsl:template>

  <xsl:template match="dicomLibrary/*">
    <xsl:choose>
      <xsl:when test="local-name()='module' or local-name()='macro' or local-name()='override'">
        <xsl:value-of select="user:indent(count(ancestor::node()))"/>
        <xsl:element name="{local-name()}">
          <xsl:attribute name="name">
            <xsl:value-of select="user:strip(@name)" />
          </xsl:attribute>
          <xsl:text>&#10;</xsl:text>
          <xsl:apply-templates select="*" />
          <xsl:value-of select="user:indent(count(ancestor::node()))"/>
        </xsl:element>
        <xsl:text>&#10;</xsl:text>
      </xsl:when>
      <xsl:when test="local-name()='elementList'">
        <xsl:value-of select="user:indent(count(ancestor::node()))"/>
        <xsl:element name="iod">
          <xsl:attribute name="name">
            <xsl:value-of select="user:strip(@name)" />
          </xsl:attribute>
          <xsl:text>&#10;</xsl:text>
          <xsl:apply-templates select="*" />
          <xsl:value-of select="user:indent(count(ancestor::node()))"/>
        </xsl:element>
        <xsl:text>&#10;</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="element">
    <xsl:value-of select="user:indent(count(ancestor::node()))"/>
    <xsl:element name="element">
      <xsl:attribute name="tag">
        <xsl:value-of select="@tag" />
      </xsl:attribute>
      <xsl:attribute name="vr">
        <xsl:value-of select="@vr" />
      </xsl:attribute>
      <xsl:attribute name="vt">
        <xsl:value-of select="@type" />
      </xsl:attribute>
      <xsl:if test="@dependency != ''">
        <xsl:attribute name="dependency">
          <xsl:value-of select="@dependency" />
        </xsl:attribute>
      </xsl:if>  
    </xsl:element>
    <xsl:text>&#10;</xsl:text>
  </xsl:template>

  <xsl:template match="sequence">
    <xsl:value-of select="user:indent(count(ancestor::node()))"/>
    <xsl:element name="sequence">
      <xsl:attribute name="tag">
        <xsl:value-of select="@tag" />
      </xsl:attribute>
      <xsl:attribute name="vr">
		  <xsl:text>SQ</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="vt">
        <xsl:value-of select="@type" />
      </xsl:attribute>
      <xsl:text>&#10;</xsl:text>
      <xsl:apply-templates select="item/*" />
      <xsl:value-of select="user:indent(count(ancestor::node()))"/>
    </xsl:element>
    <xsl:text>&#10;</xsl:text>
  </xsl:template>

  <xsl:template match="moduleRef">
    <xsl:value-of select="user:indent(count(ancestor::node()))"/>
    <xsl:element name="include">
      <xsl:attribute name="name">
        <xsl:value-of select="user:strip(@include)" />
      </xsl:attribute>
    </xsl:element>
    <xsl:text>&#10;</xsl:text>
  </xsl:template>
  
  <xsl:template match="macroRef">
    <xsl:value-of select="user:indent(count(ancestor::node()))"/>
    <xsl:element name="include">
      <xsl:attribute name="name">
        <xsl:value-of select="user:strip(@include)" />
      </xsl:attribute>
    </xsl:element>
    <xsl:text>&#10;</xsl:text>
  </xsl:template>

</xsl:stylesheet>

