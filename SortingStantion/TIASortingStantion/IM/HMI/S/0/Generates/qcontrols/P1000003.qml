import QtQuick 2.7
import "qrc:/"
IGuiPage
{
	id: q16777219
	objId: 16777219
	x: 0
	y: 0
	width: 480
	height: 272
	IGuiDiagnosticView
	{
		id: q788529152
		objId: 788529152
		x: 1
		y: 32
		width: 476
		height: 188
		qm_BorderCornerRadius: 4
		qm_BorderWidth: 1
		qm_RectangleBorder.color:"#ff636973"
		qm_FillColor: "#f7f3f7"
		qm_headerBackColor: "#f7f3f7"
		qm_toolBarBackColor: "#f7f3f7"
		qm_headerTextColor: "#ff31344a"
		qm_headerValueVarTextAlignmentHorizontal: Text.AlignLeft
		qm_headerValueVarTextAlignmentVertical: Text.AlignVCenter
		qm_headerMarginLeft: 2
		qm_headerMarginRight: 1
		qm_headerMarginBottom: 1
		qm_headerMarginTop: 1
		qm_FocusWidth: 2
		qm_FocusColor: "#ffa5cfd6"
		qm_diagViewToolbarPosX: 2
		qm_diagViewToolbarPosY: 144
		qm_diagViewToolbarWidth: 472
		qm_diagViewToolbarHeight: 42
		qm_Font.pixelSize: 13
		qm_Font.family: "Tahoma"
		qm_Font.bold: true
		qm_diagViewCornerRadius: 4
		qm_DiagnosticListComponent: [
			Component
			{
				IGuiListCtrl
				{
					id: qu788529152
					objectName: "qu788529152"
					x: 8
					y: 29
					width: 460
					height: 114
					qm_list.qm_linesPerRow: 1
					qm_list.qm_tableRowHeight: 16
					qm_list.qm_tableMarginLeft: 2
					qm_list.qm_tableMarginRight: 1
					qm_list.qm_tableMarginBottom: 1
					qm_list.qm_tableMarginTop: 1
					qm_list.qm_tableBackColor: "#ffffffff"
					qm_list.qm_tableSelectBackColor: "#ff94b6e7"
					qm_list.qm_tableAlternateBackColor: "#ffe7e7ef"
					qm_list.qm_tableTextColor: "#ff31344a"
					qm_list.qm_tableSelectTextColor: "#ffffffff"
					qm_list.qm_tableAlternateTextColor: "#ff31344a"
					qm_scrollCtrl: qus788529152

					qm_hasHeader: false
					qm_hasBorder: true
					qm_hasHorizontalScrollBar: true
					qm_hasVerticalScrollBar: true
					qm_list.qm_gridLineStyle: 0
					qm_list.qm_gridLineWidth: 1
					qm_list.qm_gridLineColor: "#ffffffff"
					qm_columnTypeList: [0, 0]
					totalColumnWidth: 462
					IGuiListScrollBarCtrl
					{
						id: qus788529152

					}
				}
			},
			Component
			{
				IGuiListCtrl
				{
					id: qu4294967295
					objectName: "qu4294967295"
					x: 8
					y: 29
					width: 460
					height: 114
					qm_list.qm_linesPerRow: 1
					qm_list.qm_tableRowHeight: 16
					qm_list.qm_tableMarginLeft: 2
					qm_list.qm_tableMarginRight: 1
					qm_list.qm_tableMarginBottom: 1
					qm_list.qm_tableMarginTop: 1
					qm_list.qm_tableBackColor: "#ffffffff"
					qm_list.qm_tableSelectBackColor: "#ff94b6e7"
					qm_list.qm_tableAlternateBackColor: "#ffe7e7ef"
					qm_list.qm_tableTextColor: "#ff31344a"
					qm_list.qm_tableSelectTextColor: "#ffffffff"
					qm_list.qm_tableAlternateTextColor: "#ff31344a"
					qm_scrollCtrl: qus4294967295

					qm_hasHeader: true
					qm_hasBorder: true
					qm_hasHorizontalScrollBar: true
					qm_hasVerticalScrollBar: true
					qm_list.qm_gridLineStyle: 0
					qm_list.qm_gridLineWidth: 1
					qm_list.qm_gridLineColor: "#ffffffff"
					qm_columnTypeList: [1, 0, 0, 0, 0]
					totalColumnWidth: 558
					qm_headerItem: qh4294967295
					IGuiListHeader
					{
						id: qh4294967295
						qm_listItem: qu4294967295
						qm_columnWidthList: [24, 24, 100, 100, 310]
						color: "#ffefebef"
						qm_tableHeaderTextColor: "#ff31344a"
						qm_tableHeaderValueVarTextAlignmentHorizontal: Text.AlignLeft
						qm_tableHeaderValueVarTextAlignmentVertical: Text.AlignVCenter
						qm_tableHeaderMarginLeft: 3
						qm_tableHeaderMarginRight: 1
						qm_tableHeaderMarginBottom: 1
						qm_tableHeaderMarginTop: 1
						qm_noOfColumns: 5
						qm_tableHeaderHeight: 16
						qm_leftImageID: 37
						qm_leftTileTop: 9
						qm_leftTileBottom: 9
						qm_leftTileRight: 2
						qm_leftTileLeft: 4
						qm_middleImageID: 38
						qm_middleTileTop: 9
						qm_middleTileBottom: 9
						qm_middleTileRight: 2
						qm_middleTileLeft: 2
						qm_rightImageID: 39
						qm_rightTileTop: 9
						qm_rightTileBottom: 9
						qm_rightTileRight: 4
						qm_rightTileLeft: 2
						radius: 2
					}
					IGuiListScrollBarCtrl
					{
						id: qus4294967295

					}
				}
			},
			Component
			{
				IGuiListCtrl
				{
					id: qu4294967294
					objectName: "qu4294967294"
					x: 8
					y: 29
					width: 460
					height: 114
					qm_list.qm_linesPerRow: 1
					qm_list.qm_tableRowHeight: 16
					qm_list.qm_tableMarginLeft: 2
					qm_list.qm_tableMarginRight: 1
					qm_list.qm_tableMarginBottom: 1
					qm_list.qm_tableMarginTop: 1
					qm_list.qm_tableBackColor: "#ffffffff"
					qm_list.qm_tableSelectBackColor: "#ff94b6e7"
					qm_list.qm_tableAlternateBackColor: "#ffe7e7ef"
					qm_list.qm_tableTextColor: "#ff31344a"
					qm_list.qm_tableSelectTextColor: "#ffffffff"
					qm_list.qm_tableAlternateTextColor: "#ff31344a"
					qm_scrollCtrl: qus4294967294

					qm_hasHeader: false
					qm_hasBorder: true
					qm_hasHorizontalScrollBar: true
					qm_hasVerticalScrollBar: true
					qm_list.qm_gridLineStyle: 0
					qm_list.qm_gridLineWidth: 1
					qm_list.qm_gridLineColor: "#ffffffff"
					qm_columnTypeList: [0]
					totalColumnWidth: 462
					IGuiListScrollBarCtrl
					{
						id: qus4294967294

					}
				}
			}
		]
		IGuiGraphicButton
		{
			id: q486539290
			objId: 486539290
			x: 56
			y: 149
			width: 44
			height: 32
			qm_BorderCornerRadius: 3
			qm_BorderWidth: 1
			qm_ImageSource: "image://QSmartImageProvider/40#2#4#128#0#0"
			qm_Border.top: 15
			qm_Border.bottom: 15
			qm_Border.right: 5
			qm_Border.left: 5
			qm_FillColor: "#ffefebef"
			qm_FocusWidth: 2
			qm_FocusColor: "#ffa5cfd6"
			qm_ImageFillMode:6
			qm_ImagePossitionX: 3
			qm_ImagePossitionY: 2
			qm_ImageWidth: 39
			qm_ImageHeight: 28
			qm_SourceSizeWidth: 39
			qm_SourceSizeHeight: 28
		}
		IGuiGraphicButton
		{
			id: q486539291
			objId: 486539291
			x: 105
			y: 149
			width: 44
			height: 32
			qm_BorderCornerRadius: 3
			qm_BorderWidth: 1
			qm_ImageSource: "image://QSmartImageProvider/40#2#4#128#0#0"
			qm_Border.top: 15
			qm_Border.bottom: 15
			qm_Border.right: 5
			qm_Border.left: 5
			qm_FillColor: "#ffefebef"
			qm_FocusWidth: 2
			qm_FocusColor: "#ffa5cfd6"
			qm_ImageFillMode:6
			qm_ImagePossitionX: 3
			qm_ImagePossitionY: 2
			qm_ImageWidth: 39
			qm_ImageHeight: 28
			qm_SourceSizeWidth: 39
			qm_SourceSizeHeight: 28
		}
		IGuiGraphicButton
		{
			id: q486539292
			objId: 486539292
			x: 154
			y: 149
			width: 44
			height: 32
			qm_BorderCornerRadius: 3
			qm_BorderWidth: 1
			qm_ImageSource: "image://QSmartImageProvider/40#2#4#128#0#0"
			qm_Border.top: 15
			qm_Border.bottom: 15
			qm_Border.right: 5
			qm_Border.left: 5
			qm_FillColor: "#ffefebef"
			qm_FocusWidth: 2
			qm_FocusColor: "#ffa5cfd6"
			qm_ImageFillMode:6
			qm_ImagePossitionX: 3
			qm_ImagePossitionY: 2
			qm_ImageWidth: 39
			qm_ImageHeight: 28
			qm_SourceSizeWidth: 39
			qm_SourceSizeHeight: 28
		}
		IGuiGraphicButton
		{
			id: q486539293
			objId: 486539293
			x: 7
			y: 149
			width: 44
			height: 32
			qm_BorderCornerRadius: 3
			qm_BorderWidth: 1
			qm_ImageSource: "image://QSmartImageProvider/40#2#4#128#0#0"
			qm_Border.top: 15
			qm_Border.bottom: 15
			qm_Border.right: 5
			qm_Border.left: 5
			qm_FillColor: "#ffefebef"
			qm_FocusWidth: 2
			qm_FocusColor: "#ffa5cfd6"
			qm_ImageFillMode:6
			qm_ImagePossitionX: 3
			qm_ImagePossitionY: 2
			qm_ImageWidth: 39
			qm_ImageHeight: 28
			qm_SourceSizeWidth: 39
			qm_SourceSizeHeight: 28
		}
		IGuiGraphicButton
		{
			id: q486539294
			objId: 486539294
			x: 154
			y: 149
			width: 44
			height: 32
			qm_BorderCornerRadius: 3
			qm_BorderWidth: 1
			qm_ImageSource: "image://QSmartImageProvider/40#2#4#128#0#0"
			qm_Border.top: 15
			qm_Border.bottom: 15
			qm_Border.right: 5
			qm_Border.left: 5
			qm_FillColor: "#ffefebef"
			qm_FocusWidth: 2
			qm_FocusColor: "#ffa5cfd6"
			qm_ImageFillMode:6
			qm_ImagePossitionX: 3
			qm_ImagePossitionY: 2
			qm_ImageWidth: 39
			qm_ImageHeight: 28
			qm_SourceSizeWidth: 39
			qm_SourceSizeHeight: 28
		}
	}
	IGuiButton
	{
		id: q486539295
		objId: 486539295
		x: 1
		y: 1
		width: 157
		height: 30
		qm_BorderCornerRadius: 3
		qm_BorderWidth: 1
		qm_ImageSource: "image://QSmartImageProvider/19#2#4#128#0#0"
		qm_Border.top: 15
		qm_Border.bottom: 15
		qm_Border.right: 5
		qm_Border.left: 5
		qm_FillColor: "#ffe7e3e7"
		qm_TextColor: "#ff31344a"
		qm_ValueVarTextAlignmentHorizontal: Text.AlignHCenter
		qm_ValueVarTextAlignmentVertical: Text.AlignVCenter
		qm_Anchors.bottomMargin: 1
		qm_Anchors.leftMargin: 1
		qm_Anchors.rightMargin: 1
		qm_Anchors.topMargin: 1
		qm_FocusWidth: 2
		qm_FocusColor: "#ff94b6e7"
	}
}
