import React from 'react';
import { View, StyleSheet, Button } from 'react-native';
import { useApp } from '../lib/hooks';
import ListItem from './ListItem';

export default function Locations()
{

    const {locations,history}=useApp();

    return (
        <View style={styles.root}>

            {locations.locations.map(loc=>(
                <ListItem title={loc.Name} key={loc.Id} onPress={()=>history.push('/location/'+loc.Id)}/>
            ))}

            <Button title="Add Location" onPress={()=>history.push('/add-location')}/>

        </View>
    )

}

const styles=StyleSheet.create({
    root:{
        flex:1
    }
});
