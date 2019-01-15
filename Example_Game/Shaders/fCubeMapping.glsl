#version 330
 
in vec3 viewDir;

uniform samplerCube s_texture;

out vec4 Color;
 
void main()
{
    Color = texture(s_texture, viewDir);
}