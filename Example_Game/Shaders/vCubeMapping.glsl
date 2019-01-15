#version 330
 
in vec3 a_Position;

uniform mat4 uView;
uniform mat4 uProj;

out vec3 viewDir;

void main()
{
    gl_Position = (vec4(a_Position * 50, 1.0) * uView * uProj);
	viewDir = a_Position;
}