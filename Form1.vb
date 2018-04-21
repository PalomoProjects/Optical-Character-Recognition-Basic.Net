
Imports Emgu.CV
Imports Emgu.CV.OCR
Imports Emgu.CV.Structure
Imports Emgu.Util

Imports System.Math
Imports System.Drawing.Imaging

Public Class Form1

    Dim Capture_Element As Capture = New Capture(1)
    Dim Image_Data As Image(Of Bgr, Byte)
    Dim OCR_Utility As Tesseract = New Tesseract("C:\Dictionary\tessdata", "eng", Tesseract.OcrEngineMode.OEM_TESSERACT_ONLY)

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Timer1.Start()
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        Image_Data = Capture_Element.RetrieveBgrFrame(0)
        PictureBox1.BackgroundImage = Image_Data.ToBitmap

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim bmp As Bitmap = PictureBox1.BackgroundImage

        bmp = CropBitmap(bmp, 150, 150, 200, 200)
        bmp = SetContrast(bmp, 1.2)
        bmp = Bringhtness_Function(bmp, 52)

        PictureBox2.BackgroundImage = bmp

        OCR_Utility.Recognize(New Image(Of Bgr, Byte)(bmp))
        Label2.Text = OCR_Utility.GetText
    End Sub




#Region "Processing Image"

    Private Function Bringhtness_Function(ByVal Image_Data As Bitmap, ByVal Brightness_Data As Integer) As Bitmap

        Dim brightness As Single = CSng(Brightness_Data / 50) ' no change in brightnes

        Dim contrast As Single = 1.0F ' no change in contrast

        Dim adjustedBrightness As Single = brightness - 1.0F
        'create matrix that will brighten and contrast the image
        Dim image_attr As New ImageAttributes
        Dim cm As ColorMatrix = New ColorMatrix(New Single()() _
            { _
            New Single() {contrast, 0.0, 0.0, 0.0, 0.0}, _
            New Single() {0.0, contrast, 0.0, 0.0, 0.0}, _
            New Single() {0.0, 0.0, contrast, 0.0, 0.0}, _
            New Single() {0.0, 0.0, 0.0, 1.0, 0.0}, _
            New Single() {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0.0, 1.0}})

        Dim rect As Rectangle = _
            Rectangle.Round(Image_Data.GetBounds(GraphicsUnit.Pixel))
        Dim wid As Integer = Image_Data.Width
        Dim hgt As Integer = Image_Data.Height

        Dim img As New Bitmap(wid, hgt)
        Dim gr As Graphics = Graphics.FromImage(img)

        image_attr.SetColorMatrix(cm)
        gr.DrawImage(Image_Data, rect, 0, 0, wid, _
            hgt, GraphicsUnit.Pixel, image_attr)

        Return img
    End Function

    Public Function SetContrast(ByVal img As Bitmap, ByVal Contrast As Double) As Bitmap
        Dim t As Double = (1.0 - Contrast) / 2.0
        Dim colormatrixval As Single()() = { _
            New Single() {Contrast, 0, 0, 0, 0}, _
            New Single() {0, Contrast, 0, 0, 0}, _
            New Single() {0, 0, Contrast, 0, 0}, _
            New Single() {0, 0, 0, 1, 0}, _
            New Single() {t, t, t, 0, 1}}
        Dim colormatrix As New Imaging.ColorMatrix(colormatrixval)
        Dim ia As New ImageAttributes
        ia.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap)
        Dim img2 As Bitmap = New Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb)
        Dim g As Graphics = Graphics.FromImage(img2)
        g.DrawImage(img, New Rectangle(0, 0, img.Width, img.Height), 0, 0, img2.Width, img2.Height, GraphicsUnit.Pixel, ia)
        Return img2
    End Function

    Private Function CropBitmap(ByRef bmp As Bitmap, ByVal cropX As Integer, ByVal cropY As Integer, ByVal cropWidth As Integer, ByVal cropHeight As Integer) As Bitmap
        Dim rect As New Rectangle(cropX, cropY, cropWidth, cropHeight)
        Dim cropped As Bitmap = bmp.Clone(rect, bmp.PixelFormat)
        Return cropped
    End Function

    Private Function RotateImg(ByVal Angle As Integer, ByVal Image_Data As Bitmap) As Bitmap
        Dim bm_in As New Bitmap(Image_Data)
        Dim wid As Single = bm_in.Width
        Dim hgt As Single = bm_in.Height

        Dim corners As PointF() = { _
                   New PointF(0, 0), _
                   New PointF(wid, 0), _
                   New PointF(0, hgt), _
                   New PointF(wid, hgt)}

        Dim cx As Single = wid / 2

        Dim cy As Single = hgt / 2

        Dim i As Integer

        For i = 0 To 3
            corners(i).X -= cx
            corners(i).Y -= cy
        Next i

        Dim theta As Double = Angle * PI / 180.0
        Dim sin_theta As Double = Sin(theta)
        Dim cos_theta As Double = Cos(theta)

        Dim X, Y As Double

        For i = 0 To 3
            X = corners(i).X
            Y = corners(i).Y
            corners(i).X = CSng(X * cos_theta + Y * sin_theta)
            corners(i).Y = CSng(-X * sin_theta + Y * cos_theta)
        Next i

        Dim xmin As Single = corners(0).X
        Dim ymin As Single = corners(0).Y

        For i = 1 To 3
            If xmin > corners(i).X Then xmin = corners(i).X
            If ymin > corners(i).Y Then ymin = corners(i).Y
        Next i

        For i = 0 To 3
            corners(i).X -= xmin
            corners(i).Y -= ymin
        Next i

        Dim bm_out As New Bitmap(CInt(-2 * xmin), CInt(-2 * ymin))
        Dim gr_out As Graphics = Graphics.FromImage(bm_out)

        ReDim Preserve corners(2)

        gr_out.DrawImage(bm_in, corners)
        Return bm_out
    End Function

#End Region

End Class
