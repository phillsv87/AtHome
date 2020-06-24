import React from 'react';
import { View, StyleSheet, SafeAreaView, ViewProps, ScrollView } from 'react-native';
import { pagePadding, lineSpacing } from '../lib/style';
import { useScrollableSource, ScrollContext } from '../CommonJs/ScrollContext';
import { useSafeArea } from '../CommonJs/SafeArea-rn';
import { PortalAnchor } from '../CommonJs/Portal-rn';

interface PageProps extends ViewProps
{
    children?:any;
    includeSafeAreas?:boolean;
    scrollable?:boolean;
    noHorizontalPadding?:boolean;
}

export default function Page({
    children,
    includeSafeAreas=true,
    noHorizontalPadding,
    style,
    scrollable,
    ...props
}:PageProps)
{

    const ss=useScrollableSource();
    if(scrollable===undefined){
        scrollable=ss.scrollable;
    }

    
    let content:any;
    if(scrollable){
        includeSafeAreas=false;
        content=(
            <ScrollView style={{flex:1,paddingHorizontal:noHorizontalPadding?0:pagePadding}}>
                <PageScrollTopPadding/>
                {children}
                <PageScrollBottomPadding/>
            </ScrollView>
        )
    }else if(includeSafeAreas){
        content=(
            <SafeAreaView style={styles.safeArea}>
                <View style={{flex:1,marginTop:lineSpacing/2}}>{children}</View>
            </SafeAreaView>
        )
    }else{
        content=children;
    }
    
        

    return (
        <ScrollContext.Provider value={ss}>
            <>
                <View style={[
                    styles.root,
                    includeSafeAreas?styles.rootSafe:null,
                    noHorizontalPadding?styles.noHorizontalPadding:null,
                    style]} {...props}>
                    {content}
                </View>
                <PortalAnchor target="page" />
            </>
        </ScrollContext.Provider>
    );
}



const styles = StyleSheet.create({
    root:{  
        flex:1,
        flexDirection:'column',
        overflow:'hidden'
    },
    rootSafe:{
        padding:pagePadding,
    },
    noHorizontalPadding:{
        paddingLeft:0,
        paddingRight:0
    },
    safeArea:{
        flex:1
    }
});

export function PageScrollTopPadding()
{
    const {top}=useSafeArea();
    return <View style={{height:lineSpacing/2+top}}/>;
}

export function PageScrollBottomPadding()
{
    const {bottom}=useSafeArea();
    return <View style={{height:bottom}}/>;
}