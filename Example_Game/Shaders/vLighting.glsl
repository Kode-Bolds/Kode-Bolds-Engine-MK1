#version 330

in vec3 a_Position;
in vec2 a_TexCoord;
in vec3 a_Normal;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;

out vec2 texCoord;
out vec3 normal;
out vec4 surfacePosition;

void main()
{
	vec4 posWorld = vec4(a_Position, 1) * uModel;
	mat3 normMat = mat3(uModel);
	normMat = inverse(normMat);
	normMat = transpose(normMat);
	normal = normalize(a_Normal*normMat);
	gl_Position = posWorld * uView * uProj;
	surfacePosition = posWorld;
	texCoord = a_TexCoord;
}