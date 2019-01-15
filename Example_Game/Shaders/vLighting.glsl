#version 330

in vec3 a_Position;
in vec2 a_TexCoord;
in vec3 a_Normal;
in vec4 eyePosition;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;
uniform vec4 lightPosition;

out vec2 texCoord;
out vec3 normal;
out vec4 surfacePosition;
out vec3 lightDir;
out vec3 viewDir;

void main()
{
	surfacePosition = vec4(a_Position, 1) * uModel;

	lightDir = normalize(lightPosition.xyz - surfacePosition.xyz); 
	viewDir = normalize(surfacePosition.xyz - eyePosition.xyz);
	normal = normalize(a_Normal);

	gl_Position = surfacePosition * uView * uProj;
	texCoord = a_TexCoord;
}