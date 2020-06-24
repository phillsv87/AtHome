import React, { useMemo } from 'react';
import { Text, Animated, View } from 'react-native';
import Page from './Page';
import { ViewRoute, ViewMatch, Navigation } from '../CommonJs/Navigation-rn';
import NavigationAnimation, { NavigationAnimationTypes, shouldUseNativeDriver } from '../CommonJs/NavigationAnimation-rn';
import { useApp } from '../lib/hooks';
import Stream from './Stream';
import HsApp from '../lib/HsApp';
import StreamList from './StreamList';

/* eslint react/display-name:0 */
function getRoutes(app:HsApp):ViewRoute[]{
    return [
        {
            path:'/',
            postRender: page,
            render:(m)=><StreamList/>
        },
        {
            match:/^\/stream\/(\d+)$/i,
            postRender: pageSlim,
            render:(m)=><Stream streamId={m.paramNumber(0)}/>
        },
    ];
}

const animationType:NavigationAnimationTypes='slide';
const nativeDriver=shouldUseNativeDriver(animationType,animationType);

function page(view: any, match: ViewMatch, direction: 'in' | 'out', animation: Animated.Value) {
    return (
        <NavigationAnimation type={animationType} animation={animation} direction={direction}>
            <Page>{view}</Page>
        </NavigationAnimation>
    )
}

function pageSlim(view: any, match: ViewMatch, direction: 'in' | 'out', animation: Animated.Value) {
    return (
        <NavigationAnimation type={animationType} animation={animation} direction={direction}>
            <View style={{flex:1,overflow:'hidden'}}>
                {view}
            </View>
        </NavigationAnimation>
    )
}

export function Router()
{
    const app=useApp();
    const routes=useMemo(()=>getRoutes(app),[app]);

    return (
        <Navigation nativeDriver={nativeDriver} routes={routes} />
    );
}