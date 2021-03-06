//////////////////////////////////////////////////////////////////////
//  created:    2010/06/06
//  filename:   GELib/Ui/UiImage.h
//  author:     League of Perfect
/// @brief
///
//////////////////////////////////////////////////////////////////////
#ifndef __GELIB_UI_UIIMAGE_H__
#define __GELIB_UI_UIIMAGE_H__

#include "UiObject.h"
#include "UiTypes.h"

namespace GELib
{

class CUiTexture;
class CUiSubTexture;

class CUiImage : public CUiObject
{
public:

	UICLASS_DECLARE(CUiImage);

	CUiImage(CUiSystem *system);
	virtual ~CUiImage();

	bool SetTexture(const char *name);
	CUiTexture* GetTexture();
	void DeleteTexture();

	// external texture or render target managed by other system
	void SetExternalTexture(int t) { m_iExternalTextureId = t; }
	int GetExternalTexture() { return  m_iExternalTextureId; }

	void SetTextureRect(int x, int y, int w, int h);
	void GetTextureRect(int &x, int &y, int &w, int &h) const;

	bool SetSubTexture(const char *texSetName, const char *subTexName);
	void SetSubTexture(CUiSubTexture *st);
	CUiSubTexture* GetSubTexture();
	void DeleteSubTexture();

	void SetRotation(float angle, float x, float y);
	void GetRotation(float &angle, float &x, float &y);

	void SetBlendMode(EUiBlendMode b) { m_eBlendMode = b; }
	EUiBlendMode GetBlendMode() { return m_eBlendMode; }

	void SetGridMode(EImageGridMode g) { m_eGridMode = g; }
	EImageGridMode GetGridMode() { return m_eGridMode; }

	void SetBorder(int left, int top, int right, int bottom);
	void GetBorder(int &left, int &top, int &right, int &bottom) const;

	void UseControlColor(bool use) { m_bUseControlColor = use; }
	bool IsUseControlColor() { return m_bUseControlColor; }

	void SetGrayscale(bool g) { m_bGrayscale = g; }
	bool IsGrayscale() { return m_bGrayscale; }

	void SetFlipX(bool flip) { m_bFlipX = flip; }
	bool IsFlipX() { return m_bFlipX; }

	void SetFlipY(bool flip) { m_bFlipY = flip; }
	bool IsFlipY() { return m_bFlipY; }

	void SetTextureIndex(int idx) { m_iTextureIndex = idx; }
	int GetTextureIndex() { return m_iTextureIndex; }

	virtual void ApplyAnimationTrack(CUiAnimationTrack &tk, float t);

    virtual bool Save(CIOStream &stream);
    virtual bool Load(CIOStream &stream);

	virtual void Draw(IPlotter2D &canvas);
	virtual void DebugDraw(IPlotter2D &canvas);

protected:

	CUiTexture *m_pTexture;
	CUiSubTexture *m_pSubTexture;
	int m_iExternalTextureId;
	bool m_bUseControlColor;
	bool m_bGrayscale;
	bool m_bFlipX;
	bool m_bFlipY;
	EUiBlendMode m_eBlendMode;
	EImageGridMode m_eGridMode;
	short m_iTexX, m_iTexY, m_iTexW, m_iTexH;
	short m_iBorderLeft, m_iBorderTop, m_iBorderRight, m_iBorderBottom;
	float m_fRotateAngle;
	float m_fRotateX, m_fRotateY;
	float m_fSpawnTime;
	int m_iTextureIndex;
};

inline void CUiImage::SetTextureRect(int x, int y, int w, int h)
{
	m_iTexX = (short)x;
	m_iTexY = (short)y;
	m_iTexW = (short)w;
	m_iTexH = (short)h;
}

inline void CUiImage::GetTextureRect(int &x, int &y, int &w, int &h) const
{
	x = m_iTexX;
	y = m_iTexY;
	w = m_iTexW;
	h = m_iTexH;
}

inline void CUiImage::SetRotation(float angle, float x=0, float y=0)	// degree
{
	m_fRotateAngle = angle;
	m_fRotateX = x;
	m_fRotateY = y;
}

inline void CUiImage::GetRotation(float &angle, float &x, float &y)		// degree
{
	angle = m_fRotateAngle;
	x = m_fRotateX;
	y = m_fRotateY;
}



}//GELib

#endif//__GELIB_UI_UIIMAGE_H__
